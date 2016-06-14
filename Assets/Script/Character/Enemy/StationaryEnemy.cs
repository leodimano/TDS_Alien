﻿using UnityEngine;
using System.Collections;

public class StationaryEnemy : BaseEnemy
{

    public float RotationSpeed;
    public float FireAngle;
    public WeaponBase[] TurretWeapons;

    private int _currentWeaponIndex = 0;
    private float _angle = 0;
    private float _dot;
    private Vector3 _targetDirection;

    void Awake()
    {
        // Wrap events
        base.OnInternalStart += this.InternalStartState;
        base.OnInternalBeforeStateMachine += InternalBeforeStateMachine;
        base.OnInternalMovingState += this.InternalMovingState;
        base.OnInternalAttackingState += this.InternalAttackingState;

        base.OnDragDownFinished += this.DragDownFinished;

    }

    void InternalStartState()
    {
        // InitializeWeapons;
        for (int i = 0; i < TurretWeapons.Length; i++)
        {
            TurretWeapons[i].WeaponOwner = this;
        }

        ChangeState(EnemyState.Creating);
    }

    #region StateMachine Events

    void InternalBeforeStateMachine()
    {
        if ((State & EnemyState.Idle) == EnemyState.Idle ||
            (State & EnemyState.Moving) == EnemyState.Moving ||
            (State & EnemyState.Attacking) == EnemyState.Attacking)
        {
            // Verifica os Jogadores que Estão no Raio de Visão
            CheckForPlayerInRange();

            // Verifica os Jogadores que podem ser Vistos
            CheckForPlayerInView();

            // Recupera os Jogadores ordenados por Agressividade e Distancia
            this.PlayersInFOV = GetPlayerWithMoreAggro();
            this.Target = this.PlayersInFOV[0].Target;

            this.ChangeState(EnemyState.Idle);

            // Tem um alvo?
            if (this.Target != null)
            {
                // Recupera a distancia entre o jogador e o inimigo
                _targetDirection = this.Target.transform.position - this.transform.position;

                // Calcula o Angulo entre o vector da frente e o vetor de direcao
                _angle = Vector3.Angle(this.transform.forward, _targetDirection);

                if (this.Target != null && _angle >= AttackAngle)
                {
                    this.ChangeState(EnemyState.Moving);
                }

                if (this.Target != null && _angle <= FireAngle)
                {
                    if (this.State == EnemyState.Moving)
                        this.ChangeState(EnemyState.Moving | EnemyState.Attacking);
                    else
                        this.ChangeState(EnemyState.Attacking);
                }
            }
        }
    }

    void InternalMovingState()
    {
        if (this.Target != null && _angle >= AttackAngle)
        {
            Vector3 cross = Vector3.Cross(this.transform.forward, _targetDirection);
            _dot = Vector3.Dot(cross, Vector3.up);
            this.transform.Rotate(new Vector3(0, RotationSpeed * Mathf.Sign(_dot), 0) * Time.deltaTime);
        }
    }

    void InternalAttackingState()
    {
        if (TurretWeapons != null && TurretWeapons.Length > 0)
        {
            if (_currentWeaponIndex >= TurretWeapons.Length)
                _currentWeaponIndex = 0;

            TurretWeapons[_currentWeaponIndex].TriggerPressed();
            TurretWeapons[_currentWeaponIndex].IsShooting = false;
            _currentWeaponIndex++;
        }
    }

    #endregion

    #region Misc Events 

    void DragDownFinished()
    {
        Destroy(this.gameObject);
    }

    #endregion

}
