using System.Collections.Generic;
using System.Linq;
using TitanReach_Server.Model;
using static TitanReach_Server.Database;

namespace TitanReach_Server.Quests {
    public class QuestState {
        public int Number { get; private set; }

        public Dictionary<int, List<QuestStep>> Options { get; private set; }

        public QuestState() {
            Number = 0;
            Options = new Dictionary<int, List<QuestStep>>();
        }

        public void AddStep(int nextStep, string text) {
            if(!Options.ContainsKey(nextStep)) {
                Options.Add(nextStep, new List<QuestStep>());
            }
            Options[nextStep].Add(new QuestStep(text));
        }

        public void CompleteStep(int step, int nextState, Player player) {
            List<QuestStep> optionSteps = Options[nextState];
            optionSteps[step].complete = true;

            if (optionSteps.Where(o => o.complete == false).Count() == 0) {
                if(nextState > Number) {
                    UpdateState(nextState);
                }
                player.NetworkActions.PlaySound("quest_updated");
            }

        }

        private void UpdateState(int state) {
            Number = state;
            Options.Clear();
        }

        public void ForceState(int state) {
            UpdateState(state);
        }

        public void LoadSteps(List<QuestStepData> questStepDatas) {
            foreach(QuestStepData questStepData in questStepDatas) {
                if (questStepData.stepDescription == null) {
                    continue;
                }
                if (!Options.ContainsKey(questStepData.nextState)) {
                    Options.Add(questStepData.nextState, new List<QuestStep>());
                }
                Options[questStepData.nextState].Add(new QuestStep(questStepData.step, questStepData.stepDescription, questStepData.isStepComplete == 1));
            }
        }

        public void UnCompleteStep(int nextState, int step) {
            Options[nextState][step].complete = false;
        }

        public List<List<QuestStep>> GetTaskOptions() {
            return Options.Values.ToList();
        } 

    }
}
