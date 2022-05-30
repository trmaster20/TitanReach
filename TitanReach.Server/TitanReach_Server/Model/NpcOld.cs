//using System;
//using System.Collections.Generic;
//using System.Linq;
//using System.Text;
//using System.Threading.Tasks;
//using TitanReach_Server.Model.Definitions;
//using TitanReach_Server.Network;
//using TitanReach_Server.Scriptable;

//namespace TitanReach_Server.Model
//{
//    public class NpcOld
//    {

//        public enum IconType
//        {
//            SHOP, EXIT, QUESTION, ACCEPT
//        }

//        public class DialogOption
//        {
//            public DialogOption(Npc.IconType icon, string text)
//            {
//                this.Icon = icon;
//                this.Text = text;
//            }
//            public Npc.IconType Icon;
//            public string Text;
//        }
//        static int NPC_COUNTER = 2000;
//        public int UID;
//        public int ID;
//        public int NpcSpawnID;
//        public int NpcDeathTime = -1;
//        public int NPCDeathAnimationLength = 3000;
//        public double Scale = 1;


//        public int LastInteractAction = Environment.TickCount;
//        public bool Busy = false;

//        //public int health;
//        public bool Dead = false;
//        public bool Removed = false;

//        float maxArrowLoadTime = 1000f;
//        float maxArrowReleaseTime = 200f;
//        float arrowSpeed = 20.0f;
//        float argoShootDistance = 20.0f;

//        public float AgroDistance = 15.0f;
//        public float DeAgroDistance = 25.0f;
//        public float MinAgroTime = 5000;

//        private int attackTime = 1250;

//        private float combatDistance = 4.0f;
//        private float maxAttackRange = 2f;
//        private float minAttackRange = 1f;
//        private int lastAttackTime = 0;
//        private int attackWaitTime = 2000;

//        private Player agroTarget = null;
//        public float distnceToAgroTarget = 0.0f;



//        public float CurrentSpeed;
//        //public float WalkingSpeed = 4.0f;
//        //public float ChasingSpeed = 6.0f;


//        public Transform transform;
//        public Vector3 stateTargetPosition = null;
//        public Vector3 stateStartPosition = null;


//        public float spawnDistance = 0;
//        public float distance = 0;
//        public float idleTime = 3000;
//        public float maxIdleTime = 3000;
//        public int totalStateTime = 0;
//        public int startTime = 0;

//        public int deadTime = 3000;
//        public int removedTime = 3000;

//        StateMachine stateMachine;
//        NPCStates currentState = NPCStates.IDLE;
//        int currentStateTime = 0;
//        int stateStartTime = 0;
//        bool damaged = false;
//        bool triggerAttack = false;
//        public int CurrentHealth;
//        public Behaviour behaviour = null;

//        public int HealthPercentage()
//        {
//            return (int)((double)CurrentHealth * 100.0 / Definition.MaxHealth);
//        }

//        public bool HasAI()
//        {
//            return behaviour != null;
//        }

//        public bool Damage(int damage, Player attacker)
//        {

//            if (Dead || CurrentHealth <= 0)
//            {

//            }
//            else
//            {
//                if (HasAI())
//                {
//                    int dmg = behaviour.OnAttack(attacker, damage);
//                    if (dmg != -1)
//                    {
//                        damage = dmg;
//                    }

//                }
//                CurrentHealth = CurrentHealth - damage;


//            }
//            damaged = true;

//            agroTarget = attacker;
//            SendTargetPosition(new Vector3(0, 0, 0), true, agroTarget.UID, -1);
//            foreach (Player pla in Server.Instance.players)
//            {
//                pla.NetworkActions.UpdateNpc(this, attacker);
//            }
//            if (!SoundSent && CurrentHealth <= 0 && !Dead)
//            {
//                attacker.NetworkActions.PlaySound("wolfdie");
//                SoundSent = true;
//            }

//            if (Dead || CurrentHealth <= 0)
//                return false;

//            return true;
//        }

//        public void CheckAndLoadAI()
//        {

//            if (DataManager.NpcBehaviours.ContainsKey(ID))
//            {
//                string className = DataManager.NpcBehaviours[ID];
//                Behaviour pkt = Server.assembly.CreateInstance(className) as Behaviour;
//                this.behaviour = pkt;
//                pkt.InitBehaviour(this);
//            }
//        }

//        public bool SoundSent = false;
//        public Npc(int id)
//        {
//            ID = id;
//            UID = NPC_COUNTER;
//            NPC_COUNTER++;
//            transform = new Transform(Vector3.Zero(), Vector3.Zero(), Vector3.Zero());
//            CheckAndLoadAI();
//            stateMachine = new StateMachine();
//            InitStates();
//            InitStateTransitions();

//            //if (Definition.NpcID == 9)
//            //{
//            //    InitArcheryStates();
//            //    InitArcheryTransitions();
//            //}

//        }



//        public void Update()
//        {
//            if (agroTarget != null)
//                if (agroTarget.Dead)
//                {
//                    SetState(NPCStates.IDLE);
//                    agroTarget = null;
//                }
//            if (Busy && Environment.TickCount - LastInteractAction > 30000)
//                Busy = false;


//            currentStateTime = Environment.TickCount - stateStartTime;
//            stateMachine.Run();


//            damaged = false;

//        }

//        public NpcDefinition Definition
//        {
//            get
//            {
//                return DataManager.NpcDefinitions[ID];
//            }
//        }

//        public NpcSpawnDefinition NpcSpawnDefinition
//        {
//            get
//            {
//                return DataManager.NpcSpawnDefinitions[NpcSpawnID];
//            }
//        }
//        int iii = 0;
//        public void SendAnimation(int state)
//        {

//            foreach (Player pl in Server.Instance.players)
//            {
//                if (pl.Viewport.NpcsInView.Contains(this))
//                {
//                    ++iii;
//                    // Server.Log("sending animation update: " + iii);
//                    pl.NetworkActions.SendNpcAnimation(state, this);

//                }
//            }
//        }

//        private void InitStates()
//        {
//            State walkingState = new State();
//            walkingState.SetOnEntry(OnWalkingEntry);
//            walkingState.SetOn(OnWalking);
//            walkingState.SetOnExit(OnWalkingExit);
//            stateMachine.RegisterState((uint)NPCStates.WALK, walkingState);

//            if (!this.Definition.Friendly)
//            {
//                State chasingState = new State();
//                chasingState.SetOnEntry(OnChaseEntry);
//                chasingState.SetOn(OnChase);
//                chasingState.SetOnExit(OnChaseExit);
//                stateMachine.RegisterState((uint)NPCStates.CHASE, chasingState);

//                State attackingState = new State();
//                attackingState.SetOnEntry(OnAttackEntry);
//                attackingState.SetOn(OnAttack);
//                attackingState.SetOnExit(OnAttackExit);
//                stateMachine.RegisterState((uint)NPCStates.ATTACK, attackingState);

//                State deadState = new State();
//                deadState.SetOnEntry(OnDeadEntry);
//                deadState.SetOn(OnDead);
//                deadState.SetOnExit(OnDeadExit);
//                stateMachine.RegisterState((uint)NPCStates.DIE, deadState);
//            }

//            if (!this.Definition.IsHumanBase)
//            {
//                State combatStrafeState = new State();
//                combatStrafeState.SetOnEntry(OnCombatStrafeEntry);
//                combatStrafeState.SetOn(OnCombatStrafe);
//                combatStrafeState.SetOnExit(OnCombatStrafeExit);
//                stateMachine.RegisterState((uint)NPCStates.COMBAT_STRAFE, combatStrafeState);

//                State combatIdleState = new State();
//                combatIdleState.SetOnEntry(OnCombatIdleEntry);
//                combatIdleState.SetOn(OnCombatIdle);
//                combatIdleState.SetOnExit(OnCombatIdleExit);
//                stateMachine.RegisterState((uint)NPCStates.COMBAT_IDLE, combatIdleState);
//            }

//            State removedState = new State();
//            removedState.SetOnEntry(OnRemovedEntry);
//            removedState.SetOn(OnRemoved);
//            removedState.SetOnExit(OnRemovedExit);
//            stateMachine.RegisterState((uint)NPCStates.REMOVED, removedState);

//            State idleState = new State();
//            idleState.SetOnEntry(OnIdleEntry);
//            idleState.SetOn(OnIdle);
//            idleState.SetOnExit(OnIdleExit);
//            stateMachine.RegisterState((uint)NPCStates.IDLE, idleState);
//        }

//        private void InitArcheryStates()
//        {
//            State arrowLoadState = new State();
//            arrowLoadState.SetOnEntry(OnArrowLoadEntry);
//            arrowLoadState.SetOn(OnArrowLoad);
//            arrowLoadState.SetOnExit(OnArrowLoadExit);
//            stateMachine.RegisterState((uint)NPCStates.LOAD_ARROW, arrowLoadState);

//            //State arrowHoldState = new State();
//            //arrowHoldState.SetOnEntry(OnArrowHoldEntry);
//            //arrowHoldState.SetOn(OnArrowHold);
//            //arrowHoldState.SetOnExit(OnArrowHoldExit);
//            //stateMachine.RegisterState((uint)NPCStates.HOLD_ARROW, arrowHoldState);

//            State arrowReleaseState = new State();
//            arrowReleaseState.SetOnEntry(OnArrowReleaseEntry);
//            arrowReleaseState.SetOn(OnArrowRelease);
//            arrowReleaseState.SetOnExit(OnArrowReleaseExit);
//            stateMachine.RegisterState((uint)NPCStates.RELEASE_ARROW, arrowReleaseState);
//        }

//        void InitStateTransitions()
//        {


//            stateMachine.RegisterTransition((uint)NPCStates.IDLE, (uint)NPCStates.DIE, () =>
//            {
//                return CurrentHealth <= 0;
//            });

//            stateMachine.RegisterTransition((uint)NPCStates.WALK, (uint)NPCStates.DIE, () =>
//            {
//                return CurrentHealth <= 0;
//            });

//            stateMachine.RegisterTransition((uint)NPCStates.CHASE, (uint)NPCStates.DIE, () =>
//            {
//                return CurrentHealth <= 0;
//            });

//            stateMachine.RegisterTransition((uint)NPCStates.ATTACK, (uint)NPCStates.DIE, () =>
//            {
//                return CurrentHealth <= 0;
//            });

//            //Idle - Walk
//            stateMachine.RegisterTransition((uint)NPCStates.IDLE, (uint)NPCStates.WALK, () =>
//            {
//                return currentStateTime > idleTime && !Busy && NpcSpawnDefinition.CanMove;
//            });

//            stateMachine.RegisterTransition((uint)NPCStates.WALK, (uint)NPCStates.IDLE, () =>
//            {
//                return currentStateTime > totalStateTime;
//            });

//            //stateMachine.RegisterTransition((uint)NPCStates.ATTACK, (uint)NPCStates.IDLE, () =>
//            //{
//            //    return true;
//            //});


//            // Chase
//            stateMachine.RegisterTransition((uint)NPCStates.IDLE, (uint)NPCStates.CHASE, () =>
//            {
//                return (CheckForAgroTargets() || damaged) && (agroTarget != null);
//            });

//            stateMachine.RegisterTransition((uint)NPCStates.WALK, (uint)NPCStates.CHASE, () =>
//            {
//                return (CheckForAgroTargets() || damaged) && (agroTarget != null);
//            });

//            stateMachine.RegisterTransition((uint)NPCStates.CHASE, (uint)NPCStates.IDLE, () =>
//            {
//                return agroTarget == null || ((distnceToAgroTarget > DeAgroDistance) || !Server.Instance.players.Contains(agroTarget)) && currentStateTime > MinAgroTime;
//            });

//            // Attack
//            stateMachine.RegisterTransition((uint)NPCStates.CHASE, (uint)NPCStates.ATTACK, () =>
//            {
//                if (triggerAttack)
//                {
//                    triggerAttack = false;
//                    return true;
//                }
//                return false;// (transform.position - agroTarget.transform.position).Magnitude() < maxAttackRange;// || triggerAttack;// && (currentStateTime >= attackWaitTime);
//            });

//            stateMachine.RegisterTransition((uint)NPCStates.ATTACK, (uint)NPCStates.CHASE, () =>
//            {
//                return (currentStateTime >= attackTime);
//            });



//            // Death
//            stateMachine.RegisterTransition((uint)NPCStates.DIE, (uint)NPCStates.REMOVED, () =>
//            {
//                return currentStateTime >= deadTime;
//            });

//            stateMachine.RegisterTransition((uint)NPCStates.REMOVED, (uint)NPCStates.IDLE, () =>
//            {
//                return currentStateTime >= NpcSpawnDefinition.RespawnTime;
//            });
//        }

//        private void InitArcheryTransitions()
//        {
//            stateMachine.RegisterTransition((uint)NPCStates.CHASE, (uint)NPCStates.LOAD_ARROW, () =>
//            {
//                return distnceToAgroTarget <= argoShootDistance;
//            });

//            stateMachine.RegisterTransition((uint)NPCStates.LOAD_ARROW, (uint)NPCStates.RELEASE_ARROW, () =>
//            {
//                return currentStateTime >= maxArrowLoadTime;
//            });

//            //stateMachine.RegisterTransition((uint)NPCStates.HOLD_ARROW, (uint)NPCStates.RELEASE_ARROW, () =>
//            //{
//            //    return currentStateTime >= maxArrowLoadTime / 2.0f;
//            //});

//            stateMachine.RegisterTransition((uint)NPCStates.RELEASE_ARROW, (uint)NPCStates.CHASE, () =>
//            {
//                return currentStateTime >= maxArrowReleaseTime;
//            });
//        }

//        private void InitHumanTransitions()
//        {

//        }

//        void OnIdleEntry()
//        {
//            if (Dead)
//            {

//            }


//            SetState(NPCStates.IDLE);
//            currentStateTime = 0;
//            agroTarget = null;

//        }

//        void OnIdle()
//        {
//        }

//        void OnIdleExit()
//        {

//        }

//        void OnWalkingEntry()
//        {
//            //if (Busy && Environment.TickCount - LastInteractAction > 30000)
//            //    Busy = false;
//            //if (Busy)
//            //{
//            //    SetState(NPCStates.IDLE);
//            //    currentStateTime = 0;
//            //    return;
//            //} else
//            //{

//            //}
//            SetState(NPCStates.WALK);
//            currentStateTime = 0;
//            idleTime = DataManager.rand.Next(2000, 10000);
//            agroTarget = null;
//            int tx = 0;
//            int tz = 0;
//            do
//            {
//                tx = DataManager.rand.Next((int)NpcSpawnDefinition.AreaMinX, (int)NpcSpawnDefinition.AreaMaxX);
//                tz = DataManager.rand.Next((int)NpcSpawnDefinition.AreaMinZ, (int)NpcSpawnDefinition.AreaMaxZ);

//                if ((float)DataManager.GetHeight(tx, tz) > 20000)
//                {
//                    Server.Log("invalid");

//                    // Server.Log((float)DataManager.GetHeight(tx, tz));
//                    //  Server.Log(tz);
//                    //  Server.Log(tx);

//                }
//                stateTargetPosition = new Vector3(tx, DataManager.GetHeight(tx, tz), tz);
//                stateStartPosition = transform.position;
//                distance = (float)Math.Sqrt(Math.Pow(stateTargetPosition.X - stateStartPosition.X, 2) + Math.Pow(stateTargetPosition.Z - stateStartPosition.Z, 2));
//                totalStateTime = (int)((distance / Definition.MoveSpeed) * 1000.0f);

//                startTime = Environment.TickCount;
//            } while (totalStateTime < 100);


//            SendTargetPosition(stateTargetPosition, false, 0, totalStateTime);





//            //SendAnimation(1); send animations in SetStaet
//        }

//        void OnWalking()
//        {

//            transform.position = Lerp(stateStartPosition, stateTargetPosition, ((float)(Environment.TickCount - startTime)) / ((float)(totalStateTime)));
//            SendExactPosition();
//        }


//        void OnWalkingExit()
//        {
//            if ((((float)(Environment.TickCount - startTime)) / ((float)(totalStateTime))) > 1)
//            {
//                transform.position = Lerp(stateStartPosition, stateTargetPosition, 1);

//            }
//            else transform.position = Lerp(stateStartPosition, stateTargetPosition, ((float)(Environment.TickCount - startTime)) / ((float)(totalStateTime)));
//            SendExactPosition();
//        }

//        void chase()
//        {
//            if (agroTarget == null)
//            {
//                return;
//            }
//            stateStartPosition = transform.position;
//            Vector3 moveVector = agroTarget.transform.position - stateStartPosition;
//            distnceToAgroTarget = moveVector.Magnitude();
//            if (moveVector.Magnitude() <= maxAttackRange)
//            {
//                Server.Log("Too Close!");
//                stateTargetPosition = stateStartPosition;// + moveVector * 0.01f;
//                return;
//            }
//            moveVector = moveVector * ((moveVector.Magnitude() - maxAttackRange) / moveVector.Magnitude());
//            stateTargetPosition = stateStartPosition + moveVector;
//            totalStateTime = (int)((moveVector.Magnitude() / (Definition.MoveSpeed * 1.5f)) * 1000.0f);
//            startTime = Environment.TickCount;
//            string print = "totalStateTime  " + totalStateTime;
//            SendTargetPosition(stateTargetPosition, true, agroTarget.UID, totalStateTime);
//        }

//        void OnChaseEntry()
//        {
//            //triggerAttack = false;

//            SetState(NPCStates.CHASE);
//            currentStateTime = 0;

//            chase();
//        }

//        void OnChase()
//        {

//            if (agroTarget == null) return;

//            Vector3 moveVector = agroTarget.transform.position - transform.position;
//            distnceToAgroTarget = moveVector.Magnitude();

//            if (distnceToAgroTarget < maxAttackRange)
//            {
//                triggerAttack = true;
//                return;
//            }


//            totalStateTime = (int)((moveVector.Magnitude() / (Definition.MoveSpeed * 1.5f)) * 1000.0f);
//            transform.position = Lerp(transform.position, agroTarget.transform.position, (200.0f) / ((float)(totalStateTime)));
//            SendTargetPosition(transform.position, true, agroTarget.UID, totalStateTime);
//            SendExactPosition();


//            //SendExactPosition();

//            //var newPos = Lerp(stateStartPosition, stateTargetPosition, ((float)(Environment.TickCount - startTime)) / ((float)(totalStateTime)));
//            //if((newPos- agroTarget.transform.position).Magnitude() > maxAttackRange)
//            //{
//            //    transform.position = newPos;
//            //}
//            //else
//            //{
//            //    //triggerAttack = true;
//            //}


//            //chase();



//        }

//        void OnChaseExit()
//        {
//            //if (agroTarget == null)
//            //    return;
//            //if ((((float)(Environment.TickCount - startTime)) / ((float)(totalStateTime))) > 1)
//            //{
//            //    var newPos = Lerp(stateStartPosition, stateTargetPosition, 1);
//            //    if ((newPos - agroTarget.transform.position).Magnitude() > maxAttackRange)
//            //    {
//            //        transform.position = newPos;
//            //    }
//            //    else
//            //    {
//            //        //triggerAttack = true;
//            //    }

//            //}
//            //else
//            //{
//            //    var newPos = Lerp(stateStartPosition, stateTargetPosition, ((float)(Environment.TickCount - startTime)) / ((float)(totalStateTime)));
//            //    if ((newPos - agroTarget.transform.position).Magnitude() > maxAttackRange)
//            //    {
//            //        transform.position = newPos;
//            //    }
//            //    else
//            //    {
//            //        //triggerAttack = true;
//            //    }
//            //}
//            //SendExactPosition();
//            SendDeAgro();

//        }

//        Vector3 distanceToAgroTarget;
//        void OnCombatStrafeEntry()
//        {
//            distanceToAgroTarget = agroTarget.transform.position - transform.position;

//        }

//        void OnCombatStrafe()
//        {

//        }

//        void OnCombatStrafeExit()
//        {

//        }

//        void OnCombatIdleEntry()
//        {

//        }

//        void OnCombatIdle()
//        {

//        }

//        void OnCombatIdleExit()
//        {

//        }

//        bool collisionCheck = false;
//        float attackRange = 3.0f;
//        void OnAttackEntry()
//        {
//            SetState(NPCStates.ATTACK);
//            currentStateTime = 0;
//            lastAttackTime = Environment.TickCount;
//            //triggerAttack = false;
//            collisionCheck = false;
//        }

//        void OnAttack()
//        {
//            if (!collisionCheck)
//            {
//                if (currentStateTime >= 300)
//                {
//                    if (agroTarget != null)
//                    {
//                        if (!agroTarget.Dead)
//                        {
//                            if ((agroTarget.transform.position - transform.position).Magnitude() < attackRange)
//                            {
//                                int damage = Utilities.DamageCalculator.NPCAttackPlayer(this, agroTarget);
//                                //var dmg = 1;
//                                agroTarget.Damage(damage, null);

//                            }
//                        }
//                    }
//                    collisionCheck = true;
//                }
//            }
//        }

//        void OnAttackExit()
//        {

//        }

//        void OnArrowLoadEntry()
//        {
//            if (agroTarget == null)
//                return;
//            SetState(NPCStates.LOAD_ARROW);
//            currentStateTime = 0;

//            foreach (Player pl in Server.Instance.players)
//            {
//                pl.NetworkActions.SendNpcTarget(this, agroTarget);

//            }
//        }

//        void OnArrowLoad()
//        {


//        }

//        void OnArrowLoadExit()
//        {

//        }

//        void OnArrowHoldEntry()
//        {
//            SetState(NPCStates.HOLD_ARROW);
//            currentStateTime = 0;

//        }

//        void OnArrowHold()
//        {
//            Server.Log("ARROW HOLD");

//        }

//        void OnArrowHoldExit()
//        {

//        }

//        void OnArrowReleaseEntry()
//        {
//            if (agroTarget == null)
//                return;
//            SetState(NPCStates.RELEASE_ARROW);
//            currentStateTime = 0;

//            Vector3 startPos = transform.position + new Vector3(0, 1, 0);
//            Vector3 direction = (agroTarget.transform.position + new Vector3(0, 1, 0) - startPos);
//            direction.Normalize();
//            direction = direction * (arrowSpeed / 20.0f);
//            Projectile arrow = new Projectile(0);

//            foreach (Player pl in Server.Instance.players)
//            {
//                pl.NetworkActions.SendNPCProjectile(arrow.Definition, direction, this);

//            }
//        }

//        void OnArrowRelease()
//        {

//        }

//        void OnArrowReleaseExit()
//        {

//        }

//        void OnDeadEntry()
//        {
//            Server.Log("NPC HAS DIED");
//            SetState(NPCStates.DIE);
//            currentStateTime = 0;

//        }

//        void OnDead()
//        {
//            if (HasAI())
//            {
//                behaviour.OnDeath();
//            }
//        }

//        void OnDeadExit()
//        {
//            Server.Log("NPC HAS RESED");

//        }

//        void OnRemovedEntry()
//        {
//            SetState(NPCStates.DIE); // REMOVED
//            currentStateTime = 0;

//            Remove();
//        }

//        void OnRemoved()
//        {
//            CurrentHealth = Definition.MaxHealth;
//        }

//        void OnRemovedExit()
//        {
//            Respawn();
//            if (HasAI())
//            {
//                CheckAndLoadAI();
//                behaviour.OnSpawn();
//            }


//        }

//        private void SetState(NPCStates state)
//        {
//            currentState = state;
//            stateStartTime = Environment.TickCount;
//            SendAnimation((int)state);

//        }



//        private bool CheckForAgroTargets()
//        {
//            if (!this.Definition.Aggressive)
//                return false;
//            float clostestPlayerDistance = 100000f;
//            if (agroTarget != null) return false;
//            foreach (Player pl in Server.Instance.players)
//            {
//                if (pl.Viewport.NpcsInView.Contains(this))
//                {
//                    float ditanceToPlayer = (pl.transform.position - transform.position).Magnitude();

//                    if (ditanceToPlayer <= AgroDistance && !pl.Dead)
//                    {
//                        clostestPlayerDistance = ditanceToPlayer;
//                        agroTarget = pl;
//                    }
//                }
//            }
//            if (agroTarget != null)
//            {
//                return true;
//            }
//            else
//            {
//                return false;
//            }
//        }

//        public void AddItemToGround(GroundItem groundItem)   //TODO move this to where it should go
//        {
//            Server.Instance.groundItems.Add(groundItem);

//            Server.Instance.Delay(60000, (timer) =>
//            {
//                if (groundItem != null && Server.Instance.groundItems.Contains(groundItem))
//                {
//                    Server.Instance.groundItems.Remove(groundItem);
//                    foreach (Player pl in Server.Instance.players)
//                    {

//                        pl.Viewport.groundItemsInView.Remove(groundItem);
//                        pl.NetworkActions.RemoveGroundItem(groundItem.groundItemUID);

//                    }
//                }
//            });


//            foreach (Player pl in Server.Instance.players)
//            {
//                if (pl.Viewport.NpcsInView.Contains(this))
//                {
//                    pl.Viewport.groundItemsInView.Add(groundItem);
//                    pl.NetworkActions.AddGroundItem(groundItem);
//                }
//            }
//        }

//        public void Remove()
//        {


//            DropTable dropTable = DataManager.DropTables[Definition.DropTalbeID];
//            double pickValue = DataManager.rand.NextDouble() * dropTable.TotalWeight;

//            Server.Log("drop " + dropTable.TotalWeight);
//            Server.Log("pick " + pickValue);
//            foreach (DropTable.Drop drop in dropTable.GuaranteedDrops)
//            {
//                GroundItem droppedItem = new GroundItem(drop.ItemID);
//                droppedItem.Item.Amount = drop.Amount;
//                droppedItem.transform = new Transform(new Vector3(transform.position.X, DataManager.GetHeight((int)transform.position.X, (int)transform.position.Z), transform.position.Z), null, null);
//                AddItemToGround(droppedItem);
//            }
//            foreach (DropTable.Drop drop in dropTable.Drops)
//            {
//                if (drop.Weight >= pickValue)
//                {
//                    GroundItem droppedItem = new GroundItem(drop.ItemID);
//                    droppedItem.Item.Amount = drop.Amount;
//                    droppedItem.transform = new Transform(new Vector3(transform.position.X, DataManager.GetHeight((int)transform.position.X, (int)transform.position.Z), transform.position.Z), null, null);
//                    AddItemToGround(droppedItem);
//                    break;
//                }
//            }

//            Dead = true;
//            Removed = true;
//            foreach (Player pla in Server.Instance.players)
//            {
//                pla.NetworkActions.UpdateNpc(this, pla);
//            }


//            //int num = 0;
//            //while(true)
//            //{
//            //    num = DataManager.rand.Next(0, DataManager.ItemDefinitions.Length - 1);
//            //    if (!DataManager.ItemDefinitions[num].Legacy)
//            //        break;
//            //}
//            //var gi = new GroundItem(num);


//            //if (this.ID == 1)
//            //    gi.Item.ID = 81;
//            //if (this.ID == 10)
//            //{
//            //    if(DataManager.rand.Next(0, 100) < 50)
//            //    {
//            //        gi.Item.ID = 17;
//            //    } else
//            //    {
//            //        gi.Item.ID = 216;
//            //        gi.Item.Amount = DataManager.rand.Next(5, 12);
//            //    }
//            //}


//            //gi.transform = new Transform(new Vector3(transform.position.X, DataManager.GetHeight((int)transform.position.X, (int)transform.position.Z), transform.position.Z), null, null);
//            //Server.Instance.groundItems.Add(gi);

//            //Server.Instance.Delay(60000, (timer) =>
//            //{
//            //    if(gi != null && Server.Instance.groundItems.Contains(gi))
//            //    {
//            //        Server.Instance.groundItems.Remove(gi);
//            //        foreach (Player pl in Server.Instance.players)
//            //        {

//            //            pl.Viewport.groundItemsInView.Remove(gi);
//            //            pl.NetworkActions.RemoveGroundItem(gi.groundItemUID);

//            //        }
//            //    }
//            //});


//            //foreach (Player pl in Server.Instance.players)
//            //{
//            //    if (pl.Viewport.NpcsInView.Contains(this))
//            //    {
//            //        pl.Viewport.groundItemsInView.Add(gi);
//            //        pl.NetworkActions.AddGroundItem(gi);
//            //    }
//            //}

//        }
//        public Vector3 originalSpawnPoint;
//        public void Respawn()
//        {


//            Server.Log("Respawning NPC");
//            CurrentHealth = Definition.MaxHealth;
//            Dead = false;
//            SoundSent = false;
//            Removed = false;
//            NpcDeathTime = -1;
//            transform.position = new Vector3(originalSpawnPoint.X, originalSpawnPoint.Y, originalSpawnPoint.Z);


//            foreach (Player pla in Server.Instance.players)
//            {
//                pla.Viewport.NpcsInView.Add(this);
//                pla.NetworkActions.SyncNpc(this);
//            }


//        }

//        public int GetWAB()
//        {
//            return Definition.AttackBonus;
//        }

//        public int GetWSB()
//        {
//            return Definition.StrengthBonus;
//        }

//        public int GetADB()
//        {
//            return Definition.DefenceBonus;
//        }

//        public float Lerp(float firstFloat, float secondFloat, float ratio)
//        {
//            return firstFloat * (1 - ratio) + secondFloat * ratio;
//        }

//        public Vector3 Lerp(Vector3 firstVector, Vector3 secondVector, float ratio)
//        {
//            float retX = Lerp(firstVector.X, secondVector.X, ratio);
//            float retY = Lerp(firstVector.Y, secondVector.Y, ratio);
//            float retZ = Lerp(firstVector.Z, secondVector.Z, ratio);

//            return new Vector3(retX, retY, retZ);
//        }

//        private void SendExactPosition()
//        {


//            foreach (Player pl in Server.Instance.players)
//            {
//                if (pl.Viewport.NpcsInView.Contains(this))
//                {
//                    pl.NetworkActions.SendNpcPosition(transform.position, this);
//                }
//            }
//        }

//        private void SendDeAgro()
//        {
//            foreach (Player pl in Server.Instance.players)
//            {
//                if (pl.Viewport.NpcsInView.Contains(this))
//                {
//                    pl.NetworkActions.SendNPCDeAgro(this);
//                }
//            }
//        }
//        int ii = 0;
//        // This will send the NPCS target which the clients will use to determine Movement
//        Vector3 prevTargetPosition = Vector3.Zero();
//        private void SendTargetPosition(Vector3 position, bool chaseTarget, uint playerid, int p_totalStateTime)
//        {
//            //Server.Log("Sending thingo:");
//            //Server.Log(totalStateTime);
//            //Server.Log(position.X);
//            //Server.Log(position.Y);
//            //Server.Log(position.Z);
//            if ((prevTargetPosition - position).Magnitude() < 1) return;
//            prevTargetPosition = position;

//            foreach (Player pl in Server.Instance.players)
//            {
//                if (pl.Viewport.NpcsInView.Contains(this))
//                {
//                    ++ii;
//                    // Server.Log("sending target update :" + ii);
//                    pl.NetworkActions.SendNpcTarget(position, p_totalStateTime, this, chaseTarget, playerid);
//                }
//            }
//        }

//    }
//}

////public enum NPCStates
////{
////    IDLE,
////    WALK,
////    CHASE,
////    ATTACK,
////    DIE,
////    REMOVED
////}
////public enum NPCStates
////{

////    IDLE,
////    WALK,
////    RUN,
////    JUMP,
////    STRAFE,
////    CHASE,
////    ATTACK,
////    DIE,
////    REMOVED,
////    INTERACTING,
////    LOAD_ARROW,
////    HOLD_ARROW,
////    RELEASE_ARROW,
////    CAST_SPELL,
////    COMBAT_STRAFE,
////    COMBAT_IDLE

////}
