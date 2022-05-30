using System;
using System.Threading;
using System.Threading.Tasks;
using System.Collections.Generic;
using TitanReach_Server;

static int SCRIPT_ID = 1;

static int QUEST_ID = 1;
static string QUEST_NAME = "The Injured Hunter";
static int COMPLETED_STATE = 130;
static Dictionary<int, QuestInfo> QUEST_INFO = new Dictionary<int, QuestInfo> {
};


static int HUNTER = 54;
static int ALCHEMIST = 31;

int choice = 0;

RegisterQuest(QUEST_ID, QUEST_NAME, COMPLETED_STATE, QUEST_INFO);

RegisterPlayer(SCRIPT_ID, (player) => {
    int QS = GetQuestState(player, QUEST_ID);
    if (QS == 0) {
        AddQuestStep(player, QUEST_ID, 1, "Automatic Setup");
        CompleteStep(player, QUEST_ID, 1, 0);
        AddQuestStep(player, QUEST_ID, 2, "Talk to " + NpcLink(HUNTER));
    }
});

Func<Npc, Player, int, CancellationToken, Task<int>> HunterDialog = async (npc, player, s, token) => {
    int QS = GetQuestState(player, QUEST_ID);

    if(QS > 40 && QS < 100) {
        await Task.Run(() => Talk(player, npc, token, "Where should I look for " + NpcLink(ALCHEMIST) + "?"), token);
        await Task.Run(() => Talk(npc, player, token, "Try looking around the hill to the South."), token);
        await Task.Run(() => Talk(player, npc, token, "Thanks, I'll be right back."), token);
        return EXIT;
    }

    switch (QS) {
        case 1:
            await Task.Run(() => Talk(npc, player, token, "Help me!"), token);
            await Task.Run(() => Talk(player, npc, token, "What's wrong?"), token);
            await Task.Run(() => Talk(npc, player, token, "I was out hunting deer, when a bear attacked me."), token);
            await Task.Run(() => Talk(player, npc, token, "Bears!? Here?"), token);
            Prompt(player, npc, "Please, I need your help, I don't think I can move without your assistance.");
            choice = await Task.Run(() => Response(player, npc, token, "Stay and help " + NpcLink(HUNTER) + "?", "Yes", "No"), token);

            switch (choice) {
                case 0:
                    CompleteStep(player, QUEST_ID, 2, 0);
                    AddQuestStep(player, QUEST_ID, 30, "Talk to " + NpcLink(HUNTER));
                    return 0;
                case 1:
                    await Task.Run(() => Talk(player, npc, token, "Nah, I'm out of here."), token);
                    return EXIT;
            }
            break;

        case 2:
            await Task.Run(() => Talk(player, npc, token, "Yeah of course, how can I help?"), token);
            Prompt(player, npc, "Please go and find me some food, that might help to heal some of my injuries. Hopefully a " + ItemLink(DataManager.ItemID.CHICKEN) + " will do it.");
            choice = await Task.Run(() => Response(player, npc, token, "Offer to bring a " + ItemLink(DataManager.ItemID.CHICKEN) + " for " + NpcLink(HUNTER) + "?", "Yes", "No"), token);

            switch (choice) {
                case 0:
                    CompleteStep(player, QUEST_ID, 30, 0);
                    AddQuestStep(player, QUEST_ID, 40, "Bring a " + ItemLink(DataManager.ItemID.CHICKEN) + "  to " + NpcLink(HUNTER));
                    return EXIT;
                case 1:
                    await Task.Run(() => Talk(player, npc, token, "Sorry, I don't have any food to spare."), token);
                    return EXIT;
            }
            break;


        case 30:
            switch (s) {
                case 0:
                    Prompt(player, npc, "Have you got the " + ItemLink(DataManager.ItemID.CHICKEN) + " for me?");
                    choice = await Task.Run(() => Response(player, npc, token, "Hand over a " + ItemLink(DataManager.ItemID.CHICKEN) + "?", "Yes", "No"), token);

                    switch (choice) {
                        case 0:
                            return 1;
                        case 1:
                            await Task.Run(() => Talk(player, npc, token, "Not right now."), token);
                            return EXIT;
                    }
                    break;

                case 1:
                    if (HasInventory(player, DataManager.ItemID.CHICKEN, 1)){
                        RemoveItem(player, DataManager.ItemID.CHICKEN, 1);
                        CompleteStep(player, QUEST_ID, 40, 0);
                        AddQuestStep(player, QUEST_ID, 50, "Talk to " + NpcLink(HUNTER));
                        await Task.Run(() => Talk(player, npc, token, "Here you go."), token);
                        await Task.Run(() => Talk(npc, player, token, "*Munching noises*|\nThank you so much, that helped a bit."), token);
                        return 0;
                    } else {
                        await Task.Run(() => Talk(npc, player, token, "You don't have any " + ItemLink(DataManager.ItemID.CHICKEN) + ", please bring me some."), token);
                        await Task.Run(() => Talk(player, npc, token, "Ok, be right back."), token);
                        return EXIT;
                    }
            }
            break;

        case 40:
            await Task.Run(() => Talk(npc, player, token, "I think I'm still too weak to move. Maybe you can find somehing a bit more potent?"), token);
            await Task.Run(() => Talk(player, npc, token, "What sort of thing did you have in mind?"), token);
            await Task.Run(() => Talk(npc, player, token, "I heard there's an Alchemist, " + NpcLink(ALCHEMIST) + ", living in Glimmermoore . Maybe they will have an idea that could help?"), token);
            Prompt(player, npc, "Could you go and talk to " + NpcLink(ALCHEMIST) + " for me?");
            choice = await Task.Run(() => Response(player, npc, token, "Offer to talk to " + NpcLink(ALCHEMIST) + "?", "Yes", "No"), token);

            switch (choice) {
                case 0:
                    CompleteStep(player, QUEST_ID, 50, 0);
                    AddQuestStep(player, QUEST_ID, 60, "Talk to " + NpcLink(ALCHEMIST) + " the Alchemist");
                    await Task.Run(() => Talk(player, npc, token, "Of course I'll help"), token);
                    return 0;
                case 1:
                    await Task.Run(() => Talk(player, npc, token, "Sorry, I'm too busy right now."), token);
                    return EXIT;
            }
            break;

        case 100:
            switch (s) {
                case 0:
                    Prompt(player, npc, "Have you got the " + ItemLink(DataManager.ItemID.MINOR_HEALTH_POTION) + " for me?");
                    choice = await Task.Run(() => Response(player, npc, token, "Hand over a " + ItemLink(DataManager.ItemID.MINOR_HEALTH_POTION) + "?", "Yes", "No"), token);

                    switch (choice) {
                        case 0:
                            await Task.Run(() => Talk(player, npc, token, "Here you go."), token);
                            return 1;
                        case 1:
                            await Task.Run(() => Talk(player, npc, token, "Not right now."), token);
                            return EXIT;
                    }
                    break;

                case 1:
                    if (HasInventory(player, DataManager.ItemID.MINOR_HEALTH_POTION, 1)) {
                        RemoveItem(player, DataManager.ItemID.MINOR_HEALTH_POTION, 1);
                        CompleteStep(player, QUEST_ID, 110, 0);
                        AddQuestStep(player, QUEST_ID, 120, "Talk to " + NpcLink(HUNTER));
                        await Task.Run(() => Talk(npc, player, token, "*Slurping noises*|\nThank you so much, I feel much better now."), token);
                        return 0;
                    } else {
                        await Task.Run(() => Talk(npc, player, token, "You don't have the " + ItemLink(DataManager.ItemID.MINOR_HEALTH_POTION) + " yet, please hurry!"), token);
                        await Task.Run(() => Talk(player, npc, token, "Ok, be right back."), token);
                        return EXIT;
                    }
            }
            break;

        case 110:
            await Task.Run(() => Talk(npc, player, token, "Here, take these as a reward."), token);
            GiveStackingItem(player, DataManager.ItemID.COPPER_ARROW, 300);
            GiveExperience(player, (int)Stats.SKILLS.Alchemy, 700);
            CompleteStep(player, QUEST_ID, 120, 0);
            await Task.Run(() => Talk(player, npc, token, "Thank you!"), token);
            return EXIT;

        case 120:
            await Task.Run(() => Talk(npc, player, token, "done test"), token);
            return EXIT;
    }
    return EXIT;
};

Func<Npc, Player, int, CancellationToken, Task<int>> AlchemistDialog = async (npc, player, s, token) => {
    int QS = GetQuestState(player, QUEST_ID);

    if(QS < 50) {
        await Task.Run(() => Talk(player, npc, token, "Hi there."), token);
        await Task.Run(() => Talk(npc, player, token, "I'm busy, leave me alone."), token);
    }

    switch (QS) {
        case 50:
            await Task.Run(() => Talk(player, npc, token, "Hi there!"), token);
            await Task.Run(() => Talk(npc, player, token, "I'm busy, leave me alone"), token);
            await Task.Run(() => Talk(player, npc, token, "Please it's urgent! There's a hunter in the woods, they're badly injured."), token);
            await Task.Run(() => Talk(npc, player, token, "Oh fine, I suppose I could help you quickly.| Do you know how to make a health potion?"), token);
            Prompt(player, npc, "I can teach you if you would like?");
            choice = await Task.Run(() => Response(player, npc, token, "Learn about Alchemy?", "Yes", "No"), token);

            switch (choice) {
                case 0:
                    CompleteStep(player, QUEST_ID, 60, 0);
                    AddQuestStep(player, QUEST_ID, 70, "Talk to " + NpcLink(ALCHEMIST));
                    await Task.Run(() => Talk(player, npc, token, "Yes please."), token);
                    return 0;

                case 1:
                    await Task.Run(() => Talk(player, npc, token, "Actually, maybe not right now."), token);
                    return EXIT;
            }
            break;

        case 60:
            await Task.Run(() => Talk(npc, player, token, "Ok, there are a few steps to making a potion."), token);
            await Task.Run(() => Talk(npc, player, token, "First you need to fill a vial with water. Then you mix in a base ingredient to make an unfinished potion. Finally you brew the potion at a cauldron."), token);
            await Task.Run(() => Talk(player, npc, token, "That doesn't sound too bad..."), token);
            await Task.Run(() => Talk(npc, player, token, "Let's get started by filling a vial with water."), token);
            await Task.Run(() => Talk(npc, player, token, "Here, take this " + ItemLink(DataManager.ItemID.VIAL)), token);
            if (GiveItem(player, DataManager.ItemID.VIAL)) {
                CompleteStep(player, QUEST_ID, 70, 0);
                AddQuestStep(player, QUEST_ID, 80, "Show a " + ItemLink(DataManager.ItemID.VIAL_OF_WATER) + " to " + NpcLink(ALCHEMIST));
                return 0;
            }
            return EXIT_FULL_INV;

        case 70:
            if (HasInventory(player, DataManager.ItemID.VIAL_OF_WATER, 1)) {
                CompleteStep(player, QUEST_ID, 80, 0);
                AddQuestStep(player, QUEST_ID, 90, "Show an " + ItemLink(DataManager.ItemID.UNFINISHED_MINOR_HEALTH_POTION) + " to " + NpcLink(ALCHEMIST));
                await Task.Run(() => Talk(player, npc, token, "I have the " + ItemLink(DataManager.ItemID.VIAL_OF_WATER) + "."), token);
                return 0;
            } else {
                await Task.Run(() => Talk(npc, player, token, "You can use the well nearby to fill your " + ItemLink(DataManager.ItemID.VIAL, "s") + "."), token);
                await Task.Run(() => Talk(player, npc, token, "Ok, be right back."), token);
                return EXIT;
            }

        case 80:
            if (HasInventory(player, DataManager.ItemID.UNFINISHED_MINOR_HEALTH_POTION, 1)) {
                CompleteStep(player, QUEST_ID, 90, 0);
                AddQuestStep(player, QUEST_ID, 100, "Show a " + ItemLink(DataManager.ItemID.MINOR_HEALTH_POTION) + " to " + NpcLink(ALCHEMIST));
                await Task.Run(() => Talk(player, npc, token, "I have the " + ItemLink(DataManager.ItemID.UNFINISHED_MINOR_HEALTH_POTION) + "."), token);
                return 0;
            } else {

                await Task.Run(() => Talk(npc, player, token, "Great, next you'll want to create an " + ItemLink(DataManager.ItemID.UNFINISHED_MINOR_HEALTH_POTION) + ". You'll need to use a " + ItemLink(DataManager.ItemID.RED_MUSHROOM) + " on a " + ItemLink(DataManager.ItemID.VIAL_OF_WATER) + "."), token);
                await Task.Run(() => Talk(player, npc, token, "Ok, be right back."), token);
                return EXIT;
            }

        case 90:
            if (HasInventory(player, DataManager.ItemID.MINOR_HEALTH_POTION, 1)) {
                CompleteStep(player, QUEST_ID, 100, 0);
                AddQuestStep(player, QUEST_ID, 110, "Take the a " + ItemLink(DataManager.ItemID.MINOR_HEALTH_POTION) + " to " + NpcLink(HUNTER));
                return 0;
            } else {
                await Task.Run(() => Talk(npc, player, token, "Perfect, now you can create the " + ItemLink(DataManager.ItemID.MINOR_HEALTH_POTION) + ". Do that at a cauldron with an " + ItemLink(DataManager.ItemID.UNFINISHED_MINOR_HEALTH_POTION) + " in yur inventory."), token);
                await Task.Run(() => Talk(player, npc, token, "Ok, be right back."), token);
                return EXIT;
            }

        case 100:
            await Task.Run(() => Talk(npc, player, token, "Well done, you don't seem to be as hopeless as I thought. Quickly, go take the " + ItemLink(DataManager.ItemID.MINOR_HEALTH_POTION) + " back to " + NpcLink(HUNTER)), token);
            await Task.Run(() => Talk(player, npc, token, "Thanks for your help."), token);
            return EXIT;

    }
    return EXIT;
};

RegisterNPCQuest(HUNTER, HunterDialog, QUEST_ID);
RegisterNPCQuest(ALCHEMIST, AlchemistDialog, QUEST_ID);
