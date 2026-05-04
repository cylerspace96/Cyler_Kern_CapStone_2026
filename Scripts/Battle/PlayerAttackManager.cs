// This file is huge and handles most player actions during battle
// This file may have several outdated or unused parts b/c I revised it a lot

using System.Collections;
using UnityEngine;

public enum TurnState
{
    Idle,
    AwaitingInput,
    ComboWindow,
    QTECheck,
    BonusRounds,
    EnemyTurn,
    TurnEnd
}

// Character states to track what can and cannot be done at a certain time
public class CharacterAttackState
{
    public bool isAttacking = false;
    public AttackData currentAttack = null;
}

public class PlayerAttackManager : MonoBehaviour
{
    // Assign all this stuff in the inspector

    [Header("References")]
    public Transform enemyTarget;
    public Transform[] players;
    public EnemyAttackManager enemyAttackManager;

    [Header("Attack Settings")]
    public float moveSpeed = 5f;
     // Offset from enemy when attacking
    public float attackOffsetX = 1.5f;

    [Header("Audio")]
    public AudioSource audioSource;
    public AudioClip qteSuccessSound;

    [Header("Combo Settings")]
    public int startingInputs = 5;
    public float comboGapTime = 1.5f;

    [Header("Turn Settings")]
    public int damageThreshold = 75;
    // How much the threshold increases each time a bonus round is triggered
    public int thresholdIncrement = 50;
    public float qteGapTime = 1.5f;

    [Header("UI")]
    public BattleUIController battleUI;
    public TurnIndicator turnIndicator;
    public GameOverController gameOverController;

    // Per-character state
    CharacterAttackState[] characterStates;
    Vector3[] originalPositions;
    Animator[] animators;

    // Turn-level state
    TurnState currentState;
    int inputsRemaining;
    bool qteRunning = false;
    bool enemyTurnStarted = false;

    int[] attacksUsedPerCharacter;

    [HideInInspector] public int totalDamageThisTurn;
    // Tracks the current threshold — increases each bonus round, resets each player turn
    [HideInInspector] public int currentThreshold;
    int activeAttackerCount;
    float comboTimer;
    bool isBonusRound;

    // Life cycle of player characters

    void Start()
    {
        // Get and store OG positions so the characters know where to return
        // Gets characters ready to attack
        originalPositions = new Vector3[players.Length];
        characterStates = new CharacterAttackState[players.Length];
        attacksUsedPerCharacter = new int[players.Length];
        animators = new Animator[players.Length];

        for (int i = 0; i < players.Length; i++)
        {
            originalPositions[i] = players[i].position;
            characterStates[i] = new CharacterAttackState();
            attacksUsedPerCharacter[i] = 0;
            animators[i] = players[i].GetComponent<Animator>();
        }

        currentState = TurnState.Idle;
    }

    void Update()
    {
        // If state is comboWindow, count the timer down
        if (currentState == TurnState.ComboWindow)
        {
            comboTimer -= Time.deltaTime;
            battleUI.UpdateCombo(comboTimer);

            if (comboTimer <= 0f)
            {
                battleUI.HideCombo();

                // Checks QTE if all inputs have been used
                // If inputs remain, and player lets the combo expire turn ends
                if (inputsRemaining <= 0 && totalDamageThisTurn >= currentThreshold && !qteRunning)
                {
                    qteRunning = true;
                    currentState = TurnState.QTECheck;
                    StartCoroutine(QTESequence());
                }
                else if (!qteRunning)
                {
                    EndPlayerTurn();
                }
            }
        }
    }

    // Helps with animations
    // Checks what is currently playing and then plays an animation
    void PlayAnimation(int index, string animationName)
    {
        if (string.IsNullOrEmpty(animationName)) return;
        Animator animator = animators[index];
        if (animator == null) return;

        // Hash-based comparison is reliable regardless of layer naming
        int hash = Animator.StringToHash(animationName);
        if (animator.GetCurrentAnimatorStateInfo(0).shortNameHash == hash) return;

        animator.Play(animationName);
    }

    
    // Turn entry and exit
    // Starts player turn and all activites that come with that
    public void StartPlayerTurn()
    {
        currentState = TurnState.AwaitingInput;
        inputsRemaining = startingInputs;
        totalDamageThisTurn = 0;
        activeAttackerCount = 0;
        isBonusRound = false;
        qteRunning = false;
        enemyTurnStarted = false;

        // Reset threshold to base value at the start of each player turn
        currentThreshold = damageThreshold;

        for (int i = 0; i < players.Length; i++)
        {
            // Track attacks used per character so the round can end properly
            attacksUsedPerCharacter[i] = 0;
            characterStates[i].isAttacking = false;
            characterStates[i].currentAttack = null;

            PlayerStats stats = players[i].GetComponent<PlayerStats>();
            if (stats != null && !stats.IsDead)
                PlayAnimation(i, stats.idleAnimationName);
        }

        // Show the player turn indicator sprite
        if (turnIndicator != null)
            turnIndicator.ShowPlayerTurn();
    }

    // End the player turn
    // Waits for all animations to finish
    void EndPlayerTurn()
    {
        if (currentState == TurnState.EnemyTurn || enemyTurnStarted)
            return;

        if (activeAttackerCount > 0)
        {
            StartCoroutine(WaitForAttackersAndEnd());
            return;
        }

        // Start the enemy turn
        enemyTurnStarted = true;
        battleUI.HideCombo();
        battleUI.HideQTE();
        currentState = TurnState.EnemyTurn;

        if (turnIndicator != null)
            StartCoroutine(ShowEnemyTurnThenStart());
        else if (enemyAttackManager != null)
            enemyAttackManager.StartEnemyTurn(this);
    }

    // Plays the enemy turn indicator then the enemies attack
    IEnumerator ShowEnemyTurnThenStart()
    {
        if (turnIndicator != null)
            turnIndicator.ShowEnemyTurn();

        float waitTime = (turnIndicator != null)
            ? turnIndicator.spinInDuration + turnIndicator.holdDuration + turnIndicator.slideOutDuration
            : 0f;
        yield return new WaitForSeconds(waitTime);

        if (enemyAttackManager != null)
            enemyAttackManager.StartEnemyTurn(this);
    }

    IEnumerator WaitForAttackersAndEnd()
    {
        // Prevents player turn from ending mid animation
        yield return new WaitUntil(() => activeAttackerCount <= 0);
        EndPlayerTurn();
    }

    // Plays when enemy turn is over and starts the player turn again
    public void OnEnemyTurnComplete()
    {
        StartPlayerTurn();
    }
    

    // Handles the characters taking damage and if any characters are dead
    public void OnPlayerHit(int playerIndex, int damage)
    {
        if (playerIndex < 0 || playerIndex >= players.Length) return;

        PlayerStats stats = players[playerIndex].GetComponent<PlayerStats>();
        if (stats == null) return;

        stats.TakeDamage(damage);

        if (!stats.IsDead)
            StartCoroutine(PlayHitReaction(playerIndex, stats));
        else
            CheckPartyWipe();

        if (stats.hitSound != null && audioSource != null)
            audioSource.PlayOneShot(stats.hitSound);
    }

    // Method used to check is every character is dead, if so end the game
    void CheckPartyWipe()
    {
        foreach (Transform player in players)
        {
            if (player == null) continue;
            PlayerStats stats = player.GetComponent<PlayerStats>();
            if (stats != null && !stats.IsDead)
                // At least one player is still alive
                return;
        }

        // All players are dead, call game over
        currentState = TurnState.TurnEnd;

        if (gameOverController != null)
            gameOverController.TriggerGameOver();
    }

    // Supposed to play an animation for the player taking damage but it doesn't seem to work
    IEnumerator PlayHitReaction(int playerIndex, PlayerStats stats)
    {
        PlayAnimation(playerIndex, stats.hitAnimationName);

        yield return null;

        // Use cached animator
        Animator animator = animators[playerIndex];
        AnimatorClipInfo[] clipInfo = animator?.GetCurrentAnimatorClipInfo(0);
        float clipLength = (clipInfo != null && clipInfo.Length > 0) ? clipInfo[0].clip.length : 0.5f;

        yield return new WaitForSeconds(clipLength);
        PlayAnimation(playerIndex, stats.idleAnimationName);
    }

    // Handling input

    // Allows for attacking with an input
    // Checks for all sorts of things (states and such) so as to make sure the character can be attacking at the point where the input is recieved
    // Cancels the combo window
    public void AttackWithPlayer(int index)
    {
        if (enemyTarget == null)
        {
            return;
        }

        if (currentState != TurnState.AwaitingInput &&
            currentState != TurnState.ComboWindow &&
            currentState != TurnState.BonusRounds)
            return;

        if (inputsRemaining <= 0)
            return;

        if (characterStates[index].isAttacking)
            return;

        // Dead characters can't attack
        PlayerStats stats = players[index].GetComponent<PlayerStats>();
        if (stats != null && stats.IsDead)
            return;

        if (currentState == TurnState.ComboWindow)
        {
            battleUI.HideCombo();
            comboTimer = 0f;
        }

        inputsRemaining--;
        activeAttackerCount++;
        characterStates[index].isAttacking = true;

        StartCoroutine(AttackSequence(index));
    }

    // Attack sequence

    // Attacks the selected enemy target when an input is pressed
    // Moves character towards the enemy, plays animation, plays sound effects
    IEnumerator AttackSequence(int index)
    {
        PlayerStats stats = players[index].GetComponent<PlayerStats>();
        Transform player = players[index];

        Transform attackTarget = enemyTarget;

        if (attackTarget == null || !attackTarget.gameObject.activeInHierarchy)
        {
            activeAttackerCount--;
            characterStates[index].isAttacking = false;
            inputsRemaining++;
            yield break;
        }

        attacksUsedPerCharacter[index]++;
        int attackIndex = Mathf.Clamp(attacksUsedPerCharacter[index] - 1, 0, stats.attacks.Length - 1);
        AttackData attack = stats.attacks[attackIndex];

        if (attack == null)
        {
            activeAttackerCount--;
            characterStates[index].isAttacking = false;
            yield break;
        }

        characterStates[index].currentAttack = attack;

        // Move to enemy
        PlayAnimation(index, stats.moveAnimationName);
        Vector3 attackPos = attackTarget.position + new Vector3(-attackOffsetX, 0f, 0f);
        yield return StartCoroutine(MoveToPosition(player, attackPos));

        // Play attack animation
        PlayAnimation(index, attack.animationName);
        yield return null;

        // Use cached animator
        AnimatorClipInfo[] clipInfo = animators[index].GetCurrentAnimatorClipInfo(0);
        float clipLength = clipInfo.Length > 0 ? clipInfo[0].clip.length : 1f;

        // Deal damage across hit windows
        float windowSize = clipLength / Mathf.Max(attack.hitCount, 1);
        for (int hit = 0; hit < attack.hitCount; hit++)
        {
            yield return new WaitForSeconds(windowSize / 2f);

            if (attackTarget != null && attackTarget.gameObject.activeInHierarchy)
                DealDamage(index, attackTarget, attack);

            if (attack.attackSound != null && audioSource != null)
                audioSource.PlayOneShot(attack.attackSound);

            yield return new WaitForSeconds(windowSize / 2f);
        }

        yield return StartCoroutine(ContinueAfterAttack(index, stats));
    }

    // Deals damage based on attack, doubles it if in bonus (QTE) rounds
    void DealDamage(int index, Transform target, AttackData attack)
    {
        if (attack == null || target == null) return;
        if (target.Equals(null)) return;
        if (!target.gameObject.activeInHierarchy) return;

        BattleUnit unit = target.GetComponent<BattleUnit>();
        if (unit == null) return;

        // Nothing happens when hitting enemies that are already at 0 health
        if (unit.currentHealth <= 0) return;

        int damage = attack.damage;
        if (isBonusRound) damage *= 2;

        unit.TakeDamage(damage);
        totalDamageThisTurn += damage;
    }

    // Post-attack

    // Returns the character to the OG position, checks if an ultimate was used, clears thier states
    IEnumerator ContinueAfterAttack(int index, PlayerStats stats)
    {
        AttackData attack = characterStates[index].currentAttack;

        yield return new WaitForSeconds(attack.returnDelay);

        // Walk back
        PlayAnimation(index, stats.moveAnimationName);
        yield return StartCoroutine(MoveToPosition(players[index], originalPositions[index]));
        PlayAnimation(index, stats.idleAnimationName);

        // Clear character state
        characterStates[index].isAttacking = false;
        characterStates[index].currentAttack = null;
        activeAttackerCount--;

        // Check ultimate
        bool usedUltimate = (attacksUsedPerCharacter[index] >= stats.attacks.Length);
        if (usedUltimate)
        {
            EndPlayerTurn();
            yield break;
        }

        // Wait for other simultaneous attackers
        if (activeAttackerCount > 0)
            yield break;

        // All done, check end conditions
        if (inputsRemaining <= 0)
        {
            if (totalDamageThisTurn >= currentThreshold && !qteRunning)
            {
                qteRunning = true;
                currentState = TurnState.QTECheck;
                StartCoroutine(QTESequence());
            }
            else if (!qteRunning)
            {
                EndPlayerTurn();
            }
            yield break;
        }

        // Start combo window
        currentState = TurnState.ComboWindow;
        comboTimer = comboGapTime;
        battleUI.showCombo(comboGapTime);
    }

    //  QTE
    // Runs the QTE timer and fill image in the UI
    IEnumerator QTESequence()
    {
        float qteTimer = qteGapTime;
        battleUI.showQTE(qteTimer);
        bool success = false;

        while (qteTimer > 0f)
        {
            qteTimer -= Time.deltaTime;
            battleUI.updateQTE(qteTimer);

            if (Input.GetKeyDown(KeyCode.Space))
            {
                success = true;
                break;
            }

            yield return null;
        }

        battleUI.HideQTE();

        if (success)
        {
            if (qteSuccessSound != null && audioSource != null)
                audioSource.PlayOneShot(qteSuccessSound);

            qteRunning = false;
            enemyTurnStarted = false;

            // Raise the threshold for the next bonus round
            currentThreshold += thresholdIncrement;

            inputsRemaining = startingInputs;
            isBonusRound = true;
            totalDamageThisTurn = 0;
            currentState = TurnState.BonusRounds;
        }
        else
        {
            EndPlayerTurn();
        }
    }

    //  Movement helper, helps move characters into position
    IEnumerator MoveToPosition(Transform obj, Vector3 target)
    {
        while (Vector3.Distance(obj.position, target) > 0.05f)
        {
            obj.position = Vector3.MoveTowards(obj.position, target, moveSpeed * Time.deltaTime);
            yield return null;
        }
        obj.position = target;
    }
}