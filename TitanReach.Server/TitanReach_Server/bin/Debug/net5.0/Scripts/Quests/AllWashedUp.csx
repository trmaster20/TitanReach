using System;
using System.Threading;
using System.Threading.Tasks;
using System.Collections.Generic;
using TitanReach_Server;

static int SCRIPT_ID = 0;

static int QUEST_ID = 0;
static string QUEST_DESCRIPTION = "";
static string QUEST_NAME = "All Washed Up";
static int COMPLETED_STATE = 300;
static Dictionary<int, QuestInfo> QUEST_INFO = new Dictionary<int, QuestInfo> {
    {0, new QuestInfo(
        new List<string> { "The stranger is by the house next to Glimmermoore waterfall" }
        , "You awoke  on the shore of a lake with a stanger leaning over you. Maybe you should talk to him."
        )},
    {5, new QuestInfo(
        new List<string> {"Press 'TAB' to open your inventory",  "Left click on the axe to equip it", "Interact with a tree using 'e' to chop it down" }
        , "You've woken up on the shore of a lake with a stanger leaning over you. Maybe you should talk to him."
        )}
};

static int THE_STRANGER = 16;
static int BUCK = 17;
static int FRITZ = 18;
static int CHARSI = 19;
static int JACK = 20;
static int MURRAY = 21;
static int NIKA = 22;
static int JEN = 50;

int choice = 0;

RegisterQuest(QUEST_ID, QUEST_NAME, COMPLETED_STATE, QUEST_INFO);

RegisterPlayer(SCRIPT_ID, (player) => {
    int QS = GetQuestState(player, QUEST_ID);
    if (QS == 0) {
        AddQuestStep(player, QUEST_ID, 1, "Automatic Setup");
        CompleteStep(player, QUEST_ID, 1, 0);
        AddQuestStep(player, QUEST_ID, 2, "Tutorial Auto Start");
        CompleteStep(player, QUEST_ID, 2, 0);
        AddQuestStep(player, QUEST_ID, 30, "Talk to " + NpcLink(THE_STRANGER));
    }

    if (QS == 80) {
        RegisterSpawnObject(40, player, LightFire);
    }
    
    if (QS == 170) {
        RegisterStanceChange(1, player, StanceTest);
    }
});

Func<Npc, Player, int, CancellationToken, Task<int>> StrangerDialog = async (npc, player, s, token) => {
    int QS = GetQuestState(player, QUEST_ID);

    if (QS > 2 && QS < 50) {
        switch (s) {
            case (0):
                await Task.Run(() => Talk(npc, player, token, "Have you got those " + ItemLink(DataManager.ItemID.LOGS) + " for me yet?"), token);
                await Task.Run(() => Talk(player, npc, token, "Um, not yet."), token);
                return 1;

            case (1):
                await Task.Run(() => Talk(npc, player, token, "Talk to " + NpcLink(JACK) + ", he'll get you sorted out."), token);
                await Task.Run(() => Talk(player, npc, token, "Ok, thanks."), token);
                return EXIT;
        }
    }

    if (QS > 100) {
        await Task.Run(() => Talk(npc, player, token, "I've already taught you everything I can"), token);
        await Task.Run(() => Talk(player, npc, token, "Ok, thanks."), token);
        return EXIT;
    }

    switch (QS) {
        case 2:
            await Task.Run(() => Talk(npc, player, token, "Glad to see you're finally awake! |I was getting worried there."), token);
            await Task.Run(() => Talk(player, npc, token, "Wh--|Where am I ?"), token);
            await Task.Run(() => Talk(npc, player, token, "You washed up on the shore, must have been quite a trip down the waterfall. |I dragged you out of the water but you were out cold."), token);
            await Task.Run(() => Talk(npc, player, token, "What's your name, stranger?| Do you remember what you were doing before waking up?"), token);
            await Task.Run(() => Talk(player, npc, token, "Thank you for your help.| My name is " + GetPlayerName(player) + ".| I don't remember much else."), token);
            
            Prompt(player, npc, "Maybe some work will help you remember something. |I'm running low on wood for the fire since helping you to recover. |Do you think you could help me get some more? I'll need 5 " +  ItemLink(DataManager.ItemID.LOGS));
            choice = await Task.Run(() => Response(player, npc, token, "Can you get 5 " + ItemLink(DataManager.ItemID.LOGS) + "?", "Yes", "No"), token);

            switch (choice) {
                case 0:
                    CompleteStep(player, QUEST_ID, 30, 0);
                    AddQuestStep(player, QUEST_ID, 40, "Talk to " + NpcLink(JACK));
                    await Task.Run(() => Talk(player, npc, token, "Yes, it might help jog some memories."), token);
                    await Task.Run(() => Talk(npc, player, token, "Great, go and talk to " + NpcLink(JACK) + " he'll show you how how to gather the logs."), token);
                    await Task.Run(() => Talk(player, npc, token, "Ok, thanks."), token);
                    return EXIT;
                case 1:
                    await Task.Run(() => Talk(player, npc, token, "No thank you, I'll be ok on my own."), token);
                    return EXIT;
            }
            break;

        case 50:
            switch (s) {
                case 0:
                    Prompt(player, npc, "Have you got the " + ItemLink(DataManager.ItemID.LOGS) +" for me yet?");
                    choice = await Task.Run(() => Response(player, npc, token, "Hand over 5 " + ItemLink(DataManager.ItemID.LOGS) + "?", "Yes", "No"), token);

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
                    if (HasInventory(player, DataManager.ItemID.LOGS, 5)) {
                        RemoveItem(player, DataManager.ItemID.LOGS, 4);
                        CompleteStep(player, QUEST_ID, 60, 0);
                        AddQuestStep(player, QUEST_ID, 70, "Talk to " + NpcLink(THE_STRANGER));
                        await Task.Run(() => Talk(npc, player, token, "Great, thanks for those! Did you remember anything while you were away?"), token);
                        await Task.Run(() => Talk(player, npc, token, "No, unfortunately not. What should I do now?"), token);
                        return 0;
                    } else {
                        await Task.Run(() => Talk (npc, player, token, "You don't have enough " + ItemLink(DataManager.ItemID.LOGS) + ", go get some more!"), token);
                        await Task.Run(() => Talk (player, npc, token, "Ok, be right back."), token);
                        return EXIT;
                    }
            }
            break;

        case 60:
            Prompt(player, npc, "I've left you one of those logs, do you think you can help me to light a fire?");
            choice = await Task.Run(() => Response(player, npc, token, "Offer to help light the fire?", "Yes", "No"), token);
            switch (choice) {
                case 0:
                    await Task.Run(() => Talk(player, npc, token, "Of course I'll help!"), token);
                    await Task.Run(() => Talk(npc, player, token, "You'll need a " + ItemLink(DataManager.ItemID.FLINT) + " to light a fire. Here, you can have this spare one."), token);

                    if (GiveItem(player, DataManager.ItemID.FLINT)) {
                        CompleteStep(player, QUEST_ID, 70, 0);
                        AddQuestStep(player, QUEST_ID, 80, "Talk to " + NpcLink(THE_STRANGER));
                        await Task.Run(() => Talk(player, npc, token, "Great thanks, what do I do now?"), token);
                        return 0;
                    }
                    return EXIT_FULL_INV;

                case 1:
                    await Task.Run(() => Talk(player, npc, token, "Maybe later."), token);
                    return EXIT;
            }
            break;

        case 70:
            await Task.Run(() => Talk(npc, player, token, "Use the " + ItemLink(DataManager.ItemID.FLINT) + " on some " + ItemLink(DataManager.ItemID.LOGS) + ", and you'll light a fire."), token);
            await Task.Run(() => Talk(player, npc, token, "Thanks, I'll get that fire lit."), token);
            CompleteStep(player, QUEST_ID, 80, 0);
            AddQuestStep(player, QUEST_ID, 90, "Light a fire.");
            RegisterSpawnObject(40, player, LightFire);
            return EXIT;
        
        case 80:
            await Task.Run(() => Talk(npc, player, token, "Use the " + ItemLink(DataManager.ItemID.FLINT) + " on some " + ItemLink(DataManager.ItemID.LOGS) + ", and you'll light a fire."), token);
            await Task.Run(() => Talk(player, npc, token, "Thanks, I'll get that fire lit."), token);
            return EXIT;


        case 90:
            CompleteStep(player, QUEST_ID, 100, 0);
            AddQuestStep(player, QUEST_ID, 110, "Speak to " + NpcLink(FRITZ));
            await Task.Run(() => Talk(npc, player, token, "Awesome work, you can use fires to cook food. On that note, go and speak to " + NpcLink(FRITZ) + " on the Northern side of the lake. He might be able to help you find some food."), token);
            await Task.Run(() => Talk(player, npc, token, "Thanks for your help!"), token);
            return EXIT;

        case 100:
            await Task.Run(() => Talk(player, npc, token, "I've forgotten what I was supposed to be doing"), token);
            await Task.Run(() => Talk(npc, player, token, "You can find " + NpcLink(FRITZ) + " on the Northern side of the lake. He might be able to help you find some food."), token);
            await Task.Run(() => Talk(player, npc, token, "Thanks for your help!"), token);
            return EXIT;


    }
    return EXIT;
};

Func<Npc, Player, int, CancellationToken, Task<int>> JackLumberDialog = async (npc, player, s, token) => {
    int QS = GetQuestState(player, QUEST_ID);

    if (QS < 30) {
        await Task.Run(() => Talk(npc, player, token, "Hey you look lost, maybe you should go talk to " + NpcLink(THE_STRANGER)), token);
        await Task.Run(() => Talk(player, npc, token, "Thanks."), token);
        return EXIT;
    }

    if(QS > 50) {
        await Task.Run(() => Talk(npc, player, token, "I don't have anything left to teach you."), token);
        await Task.Run(() => Talk(player, npc, token, "See you around."), token);
        return EXIT;
    }

    switch (QS) {
        case 30:
            //player,talks to the lumberjack - gets an axe
            switch (s) {
                case 0:
                    await Task.Run(() => Talk(npc, player, token, "Hi there, how can I help?"), token);
                    await Task.Run(() => Talk(player, npc, token, NpcLink(THE_STRANGER) + " sent me, he said you could show me how to get some logs."), token);
                    return 1;

                case 1:
                    Prompt(player, npc, "Of course I can, first of all you'll need this " + ItemLink(DataManager.ItemID.COPPER_AXE));
                    choice = await Task.Run(() => Response(player, npc, token, "Take the " + ItemLink(DataManager.ItemID.COPPER_AXE) + "?", "Yes", "No"), token);

                    switch (choice) {
                        case 0:
                            if (GiveItem(player, DataManager.ItemID.COPPER_AXE)) {
                                CompleteStep(player, QUEST_ID, 40, 0);
                                AddQuestStep(player, QUEST_ID, 50, "Talk to " + NpcLink(JACK));
                                await Task.Run(() => Talk(player, npc, token, "Thank you, I will."), token);
                                await Task.Run(() => Talk(npc, player, token, "This " + ItemLink(DataManager.ItemID.COPPER_AXE) + " should sort you out."), token);
                                await Task.Run(() => Talk(player, npc, token, "Great thanks, what do I do now?"), token);
                                return 0;
                            }
                            return EXIT_FULL_INV;
                        case 1:
                            await Task.Run(() => Talk(player, npc, token, "No thanks, not right now."), token);
                            return EXIT;
                    }
                    break;
            }
            break;

        case 40:
            await Task.Run(() => Talk(npc, player, token, "You can use that " + ItemLink(DataManager.ItemID.COPPER_AXE) + " of yours to cut down some of the trees around here. Get 5 " + ItemLink(DataManager.ItemID.LOGS) + " and take them back to " + NpcLink(THE_STRANGER)), token);
            await Task.Run(() => Talk(player, npc, token, "Ok, sounds good."), token);
            await Task.Run(() => Talk(npc, player, token, "You'll have to equip your axe before you can chop a tree down. Why don't you give it a try now?| You can open your inventory by pressing \"Tab\""), token);
            await Task.Run(() => Talk(player, npc, token, "Ok, sounds good."), token);
            CompleteStep(player, QUEST_ID, 50, 0);
            AddQuestStep(player, QUEST_ID, 60, "Take 5 " + ItemLink(DataManager.ItemID.LOGS) + "to " + NpcLink(THE_STRANGER));
            return EXIT;

        case 50:
            await Task.Run(() => Talk(player, npc, token, "Can you remind me what I was meant to be doing?"), token);
            await Task.Run(() => Talk(npc, player, token, "You need to deliver 5 " + ItemLink(DataManager.ItemID.LOGS) + " to " + NpcLink(THE_STRANGER)), token);
            await Task.Run(() => Talk(player, npc, token, "Oh ok, thanks."), token);
            return EXIT;
    }
    return EXIT;
};

Func<Npc, Player, int, CancellationToken, Task<int>> BillyBassDialog = async (npc, player, s, token) => {
    int QS = GetQuestState(player,QUEST_ID);

    if(QS < 100 || QS > 130) {
        await Task.Run(() => Talk(npc, player, token, "I don't have anything I can help you with right now."), token);
        await Task.Run(() => Talk(player, npc, token, "Ok, thanks."), token);
        return EXIT;
    }

    switch (QS) {
        case 100:
            Prompt(player, npc, "Have you come to learn about fishing?");
            choice = await Task.Run(() => Response(player, npc, token, "Learn about fishing?", "Yes", "No"), token);

            switch (choice) {
                case 0:
                    await Task.Run(() => Talk(player, npc, token, "I'd love to learn about fishing!"), token);
                    await Task.Run(() => Talk(npc, player, token, "Fishing can provide you with an endless supply of food.| You can use that food to heal your health and help you survive the dangers of this world."), token);
                    await Task.Run(() => Talk(player, npc, token, "So how do I start fishing?"), token);
                    if (GiveItem(player, DataManager.ItemID.FISHING_ROD)) {
                        if (GiveStackingItem(player, DataManager.ItemID.WORMS, 15)) {
                            CompleteStep(player, QUEST_ID, 110, 0);
                            AddQuestStep(player, QUEST_ID, 120, "Show 5 " + ItemLink(DataManager.ItemID.RAW_CHUB) + " to " + NpcLink(FRITZ));
                            await Task.Run(() => Talk(npc, player, token, "You'll need this " + ItemLink(DataManager.ItemID.FISHING_ROD) + ", and some " + ItemLink(DataManager.ItemID.WORMS) + " for bait."), token);
                            await Task.Run(() => Talk(player, npc, token, "Great thanks, what do I do now?"), token);
                            return 0;
                        } else {
                            return EXIT_FULL_INV;
                        }
                    } else {
                        return EXIT_FULL_INV;
                    };
                case 1:
                    await Task.Run(() => Talk(player, npc, token, "Not right now."), token);
                    return EXIT;
            }
            break;

        case 110:
            if(HasInventory(player, DataManager.ItemID.RAW_CHUB, 5)) {
                CompleteStep(player, QUEST_ID, 120, 0);
                AddQuestStep(player, QUEST_ID, 130, "Show a " + ItemLink(DataManager.ItemID.CHUB) + " to " + NpcLink(FRITZ));
                return 0;

            } else {
                await Task.Run(() => Talk(npc, player, token, "There are some " + ItemLink(DataManager.ItemID.RAW_CHUB) + " over there in the lake. Go and catch 5 and bring them back."), token);
                await Task.Run(() => Talk(player, npc, token, "Ok, be right back."), token);
                return EXIT;
            }

        case 120:
            if (HasInventory(player, DataManager.ItemID.CHUB, 1)) {
                CompleteStep(player, QUEST_ID, 130, 0);
                AddQuestStep(player, QUEST_ID, 140, "Talk to " + NpcLink(BUCK));
                await Task.Run(() => Talk(npc, player, token, "Awesome work! I've taught you what I can for now. Why don't you go and see " + NpcLink(BUCK) + " next? You'll find him in the copper mine to the North, on the right just down the slope." +
                    " He might be able to show you how to get a weapon to help defend yourself."), token);
                await Task.Run(() => Talk(player, npc, token, "Ok, see you later."), token);
                return EXIT;

            } else {
                await Task.Run(() => Talk(npc, player, token, "Now you'll want to cook those " + ItemLink(DataManager.ItemID.RAW_CHUB) + ". Light a fire and bring back at least one cooked " + ItemLink(DataManager.ItemID.CHUB)), token);
                await Task.Run(() => Talk(player, npc, token, "Ok, be right back."), token);
                return EXIT;
            }

        case 130:
            await Task.Run(() => Talk(player, npc, token, "I've forgotten what I was supposed to be doing"), token);
            await Task.Run(() => Talk(npc, player, token, "Why don't you go and see " + NpcLink(BUCK) + " next? You'll find him in the copper mine to the North, on the right just down the slope." +
                    " He might be able to show you how to get a weapon to help defend yourself."), token);
            await Task.Run(() => Talk(player, npc, token, "Ok, see you later."), token);
                return EXIT;
    }
    return EXIT;
};

Func<Npc, Player, int, CancellationToken, Task<int>> DrillPitTaylorDialog = async (npc, player, s, token) => {
    int QS = GetQuestState(player, QUEST_ID);

    if (QS < 130 || QS > 150) {
        await Task.Run(() => Talk(npc, player, token, "I don't have anything I can help you with right now"), token);
        await Task.Run(() => Talk(player, npc, token, "Ok, thanks."), token);
        return EXIT;
    }
    switch (QS) {
        case 130:
            await Task.Run(() => Talk(player, npc, token, "Hi there, " + NpcLink(FRITZ) + " mentioned something about making a weapon?"), token);
            await Task.Run(() => Talk(npc, player, token, "Ah, a new face in town. If you're planning on getting into any trouble a weapon would definitely be a good idea."), token);
            await Task.Run(() => Talk(player, npc, token, "How would I get a weapon?"), token);
            Prompt(player, npc, "You'll need metal ingots to make a weapons, and ore to make those ingots. Luckily for you, you can mine that ore straight out of the ground." +
                " I could teach you if you would like?");

            choice = await Task.Run(() => Response(player, npc, token, "Learn about mining?", "Yes", "No"), token);

            switch (choice) {
                case 0:
                    await Task.Run(() => Talk(player, npc, token, "I'd love to learn about mining!"), token);
                    if (GiveItem(player, DataManager.ItemID.COPPER_PICKAXE)) {
                        CompleteStep(player, QUEST_ID, 140, 0);
                        AddQuestStep(player, QUEST_ID, 150, "Show 8 " + ItemLink(DataManager.ItemID.COPPER_ORE) + " to " + NpcLink(BUCK));
                        await Task.Run(() => Talk(npc, player, token, "Here, you'll need this " + ItemLink(DataManager.ItemID.COPPER_PICKAXE)), token);
                        await Task.Run(() => Talk(player, npc, token, "Great, thanks, what do I do with it?"), token);
                        return 0;
                    } else {
                        return EXIT_FULL_INV;
                    };
                case 1:
                    await Task.Run(() => Talk(player, npc, token, "Not right now."), token);
                    return EXIT;
            }
            break;

        case 140:
            if (HasInventory(player, DataManager.ItemID.COPPER_ORE, 8)) {
                CompleteStep(player, QUEST_ID, 150, 0);
                AddQuestStep(player, QUEST_ID, 151, "Talk to "+ NpcLink(CHARSI));
                await Task.Run(() => Talk(npc, player, token, "Awesome! Now you can make yourself a " + ItemLink(DataManager.ItemID.COPPER_DAGGER)), token);
                await Task.Run(() => Talk(player, npc, token, "Ok great, how do I do that?"), token);
                await Task.Run(() => Talk(npc, player, token, "There is a forge to the South, head over there and talk to " + NpcLink(CHARSI) + ". They should be able to help you out."), token);
                return EXIT;

            } else {
                await Task.Run(() => Talk(npc, player, token, "There are some copper rocks in the mine behind me, mine 8 " + ItemLink(DataManager.ItemID.COPPER_ORE) + " and come back to me."), token);
                await Task.Run(() => Talk(player, npc, token, "Ok, be right back."), token);
                return EXIT;
            }

        case 150:
            await Task.Run(() => Talk(player, npc, token, "I've forgotten what I was supposed to be doing"), token);
            await Task.Run(() => Talk(npc, player, token, "There is a forge to the South, head over there and talk to the " + NpcLink(CHARSI) + ". They should be able to help you out."), token);
            await Task.Run(() => Talk(player, npc, token, "Great, thanks for your help."), token);
            return EXIT;
    }
    return EXIT;
};

Func<Npc, Player, int, CancellationToken, Task<int>> SmithDialog = async (npc, player, s, token) => {
    int QS = GetQuestState(player, QUEST_ID);

    if (QS < 130 || QS > 160) {
        await Task.Run(() => Talk(npc, player, token, "I don't have anything I can help you with right now"), token);
        await Task.Run(() => Talk(player, npc, token, "Ok, thanks."), token);
        return EXIT;
    }

    switch (QS) {

        case 150:
            await Task.Run(() => Talk(player, npc, token, "Hi there, " + NpcLink(BUCK) + " mentioned something about making a weapon?"), token);
            await Task.Run(() => Talk(npc, player, token, "Oh I'm the right person to talk to about weapons alright. We'll soon get you sorted out."), token);
            Prompt(player, npc, "I can teach you if you would like?");

            choice = await Task.Run(() => Response(player, npc, token, "Learn about Metallurgy and Forging?", "Yes", "No"), token);

            switch (choice) {
                case 0:
                    CompleteStep(player, QUEST_ID, 151, 0);
                    AddQuestStep(player, QUEST_ID, 152, "Show 4 " + ItemLink(DataManager.ItemID.COPPER_INGOT, "s") + " to " + NpcLink(CHARSI));
                    await Task.Run(() => Talk(player, npc, token, "Yes please."), token);
                    return 0;

                case 1:
                    await Task.Run(() => Talk(player, npc, token, "Not right now."), token);
                    return EXIT;
            }
            break;

        case 151:
            if (HasInventory(player, DataManager.ItemID.COPPER_INGOT, 4)) {
                CompleteStep(player, QUEST_ID, 152, 0);
                AddQuestStep(player, QUEST_ID, 160, "Show a " + ItemLink(DataManager.ItemID.COPPER_DAGGER) + " to " + NpcLink(CHARSI));
                return 0;

            } else {
                await Task.Run(() => Talk(npc, player, token, "Great, to start you'll need to smelt some " + ItemLink(DataManager.ItemID.COPPER_ORE) + " into a " + ItemLink(DataManager.ItemID.COPPER_INGOT) + " in the furnace next to me. "), token);
                await Task.Run(() => Talk(player, npc, token, "Ok, I'll be right back."), token);
                return EXIT;
            }

        case 152:
            if (HasInventory(player, DataManager.ItemID.COPPER_DAGGER, 1)) {
                CompleteStep(player, QUEST_ID, 160, 0);
                AddQuestStep(player, QUEST_ID, 165, "Talk to " + NpcLink(MURRAY));
                await Task.Run(() => Talk(npc, player, token, "Perfect, you'll be making Titanite armour in no time. For now though, you'll need more than just that " + ItemLink(DataManager.ItemID.COPPER_DAGGER) + " to survive the dangers of the world.| Go and see the " + NpcLink(MURRAY) + " by the cliffs across the bridge, they'll help you learn to defend yourself."), token);
                await Task.Run(() => Talk(player, npc, token, "Great, thanks for your help."), token);
                return EXIT;

            } else {
                await Task.Run(() => Talk(npc, player, token, "Great work, you can finally make yourself a weapon. Let's start with something simple like a " + ItemLink(DataManager.ItemID.COPPER_DAGGER) + ".| You can craft one from a " + ItemLink(DataManager.ItemID.COPPER_INGOT) + " at the anvil next to me."), token);
                await Task.Run(() => Talk(player, npc, token, "I'm on it."), token);
                return EXIT;
            }

        case 160:
            await Task.Run(() => Talk(player, npc, token, "I've forgotten what I was supposed to be doing"), token);
            await Task.Run(() => Talk(npc, player, token, "Go and see " + NpcLink(MURRAY) + " by the tavern across the bridge, they'll help you learn to defend yourself."), token);
            await Task.Run(() => Talk(player, npc, token, "Great, thanks for your help."), token);
            return EXIT;
    }

    return EXIT;
};

Func<Npc, Player, int, CancellationToken, Task<int>> SurvivalistDialog = async (npc, player, s, token) => {
    int QS = GetQuestState(player, QUEST_ID);

    if (QS < 160 || QS > 240) {
        await Task.Run(() => Talk(npc, player, token, "I don't have anything I can help you with right now."), token);
        await Task.Run(() => Talk(player, npc, token, "Ok, thanks."), token);
        return EXIT;
    }
    switch (QS) {
        case 160:

            Prompt(player, npc, "Have you come to learn about survival?");
            choice = await Task.Run(() => Response(player, npc, token, "Learn about survival?", "Yes", "No"), token);

            switch (choice) {
                case 0:
                    CompleteStep(player, QUEST_ID, 165, 0);
                    AddQuestStep(player, QUEST_ID, 170, "Change Combat Style to Strength.");
                    RegisterStanceChange(1, player, StanceTest);

                    await Task.Run(() => Talk(player, npc, token, "I'd love to learn about survival!"), token);
                    await Task.Run(() => Talk(npc, player, token, "Well first things first, if you're going to train to fight, then you'll need to know about Combat Styles. If you press \"tab\", you can select a Combat Style." +
                        " When you're fighting an enemy, you gain experience in the skill you have selected as your Combat Style."), token);
                    await Task.Run(() => Talk(npc, player, token, "Try it out and come back to me. Change your style to Strength."), token);
                    return EXIT;

                case 1:
                    await Task.Run(() => Talk(player, npc, token, "Not right now."), token);
                    return EXIT;
            }
            break;

        case 170:
            await Task.Run(() => Talk(npc, player, token, "Your dagger is nice and all, but now that you know how to fight you might want a proper weapon." + 
                "| If you want to survive, you might want something longer range."), token);
            await Task.Run(() => Talk(npc, player, token, "I can show you how to make a " + ItemLink(DataManager.ItemID.SHORTBOW)), token);
            await Task.Run(() => Talk(player, npc, token, "That sounds great, how do I do that?"), token);
            CompleteStep(player, QUEST_ID, 180, 0);
            AddQuestStep(player, QUEST_ID, 190, "Show a " + ItemLink(DataManager.ItemID.SHORTBOW_UNSTRUNG) + " to " + NpcLink(MURRAY));
            return 0;

        case 180:
            if (HasInventory(player, DataManager.ItemID.SHORTBOW_UNSTRUNG, 1)) {
                CompleteStep(player, QUEST_ID, 190, 0);
                AddQuestStep(player, QUEST_ID, 200, "Show a " +ItemLink(DataManager.ItemID.BOW_STRING) + " to " + NpcLink(MURRAY));
                await Task.Run(() => Talk(npc, player, token, "Awesome! Next you'll need to get a " + ItemLink(DataManager.ItemID.BOW_STRING)), token);
                await Task.Run(() => Talk(player, npc, token, "Ok great, how do I do that?"), token);
                return 0;

            } else {
                await Task.Run(() => Talk(npc, player, token, "A bow is made from an unstrung bow and a bow string. Firstly go and get " + ItemLink(DataManager.ItemID.LOGS) + ", then craft them into an " + ItemLink(DataManager.ItemID.SHORTBOW_UNSTRUNG) + " at the Workbench up the path to the South East ."), token);
                await Task.Run(() => Talk(player, npc, token, "Ok, be right back"), token);
                return EXIT;
            }

        case 190:
            if (HasInventory(player, DataManager.ItemID.BOW_STRING, 1)) {
                CompleteStep(player, QUEST_ID, 200, 0);
                AddQuestStep(player, QUEST_ID, 210, "Show a " + ItemLink(DataManager.ItemID.SHORTBOW_UNSTRUNG) + " to " + NpcLink(MURRAY));
                await Task.Run(() => Talk(npc, player, token, "Awesome! Now you can string the " + ItemLink(DataManager.ItemID.SHORTBOW_UNSTRUNG)), token);
                await Task.Run(() => Talk(player, npc, token, "Ok great, how do I do that?"), token);
                return 0;

            } else {
                await Task.Run(() => Talk(npc, player, token, "Go and pick some " + ItemLink(DataManager.ItemID.FLAX) + ", then turn it into a " + ItemLink(DataManager.ItemID.BOW_STRING) + " at a spinning wheel. You can find one near the workbench."), token);
                await Task.Run(() => Talk(player, npc, token, "Ok, be right back."), token);
                return EXIT;
            }

        case 200:
            if (HasInventory(player, DataManager.ItemID.SHORTBOW, 1)) {
                CompleteStep(player, QUEST_ID, 210, 0);
                AddQuestStep(player, QUEST_ID, 220, "Show 30 " + ItemLink(DataManager.ItemID.ARROW_SHAFT, "s") + " to the " + NpcLink(MURRAY));
                await Task.Run(() => Talk(npc, player, token, "Awesome! That's a great start, but you're going to need some ammunition. I'll show you how to make some " + ItemLink(DataManager.ItemID.COPPER_ARROW, "s")), token);
                await Task.Run(() => Talk(player, npc, token, "Sounds great, where do I start?"), token);
                return 0;

            } else {
                await Task.Run(() => Talk(npc, player, token, "Use the " + ItemLink(DataManager.ItemID.BOW_STRING) + " on the " + ItemLink(DataManager.ItemID.SHORTBOW_UNSTRUNG) + " in your inventory."), token);
                await Task.Run(() => Talk(player, npc, token, "Ok, be right back."), token);
                return EXIT;
            }

        case 210:
            if (HasInventory(player, DataManager.ItemID.ARROW_SHAFT, 30)) {
                CompleteStep(player, QUEST_ID, 220, 0);
                AddQuestStep(player, QUEST_ID, 230, "Show 30 " + ItemLink(DataManager.ItemID.HEADLESS_ARROW, "s") + " to " + NpcLink(MURRAY));
                await Task.Run(() => Talk(npc, player, token, "Awesome! Next you'll need to get some " + ItemLink(DataManager.ItemID.HEADLESS_ARROW, "s")), token);
                await Task.Run(() => Talk(player, npc, token, "Ok great, how do I do that?"), token);
                return 0;

            } else {
                await Task.Run(() => Talk(npc, player, token, "Firstly go and get some " + ItemLink(DataManager.ItemID.LOGS) + ", then craft them into " + ItemLink(DataManager.ItemID.ARROW_SHAFT, "s") + " at a Workbench."), token);
                await Task.Run(() => Talk(player, npc, token, "Ok, be right back."), token);
                return EXIT;
            }

        case 220:
            if (HasInventory(player, DataManager.ItemID.HEADLESS_ARROW, 30)) {
                CompleteStep(player, QUEST_ID, 230, 0);
                AddQuestStep(player, QUEST_ID, 240, "Show 30 " + ItemLink(DataManager.ItemID.COPPER_ARROW, "s") + " to " + NpcLink(MURRAY));
                await Task.Run(() => Talk(npc, player, token, "Awesome! Now you can finish making these " + ItemLink(DataManager.ItemID.COPPER_ARROW, "s")), token);
                await Task.Run(() => Talk(player, npc, token, "Ok great, how do I do that?"), token);
                return 0;

            } else {
                await Task.Run(() => Talk(npc, player, token, ItemLink(DataManager.ItemID.HEADLESS_ARROW, "s") + " are made by using " + ItemLink(DataManager.ItemID.FEATHER, "s") + " on " + ItemLink(DataManager.ItemID.ARROW_SHAFT, "s") + ". " + NpcLink(1, "s") + " drop " + ItemLink(DataManager.ItemID.FEATHER, "s") + " when they die."), token);
                await Task.Run(() => Talk(npc, player, token, "Careful when you fight a " + NpcLink(1) + ". They're more dangerous than they sound."), token);
                await Task.Run(() => Talk(npc, player, token, "If you die then you'll drop any items you have in your inventory, you'll have to get back to where you died to retrieve your items."), token);
                await Task.Run(() => Talk(npc, player, token, "Don't forget you can dodge with 'Q', that might help you avoid some damage."), token);
                await Task.Run(() => Talk(player, npc, token, "Ok, I'll go and get those " + ItemLink(DataManager.ItemID.FEATHER, "s")), token);
                return EXIT;
            }

        case 230:
            if (HasInventory(player, DataManager.ItemID.COPPER_ARROW, 30)) {
                CompleteStep(player, QUEST_ID, 240, 0);
                AddQuestStep(player, QUEST_ID, 250, "Talk to " + NpcLink(NIKA));
                await Task.Run(() => Talk(npc, player, token, "Awesome! You look like you're sorted for weapons for the time being. " + NpcLink(NIKA) + " should be able to help you make some armour"), token);
                await Task.Run(() => Talk(player, npc, token, "Ok great."), token);
                return EXIT;

            } else {
                await Task.Run(() => Talk(npc, player, token, "You can make " + ItemLink(DataManager.ItemID.COPPER_ARROW, "s") + " by using " + ItemLink(DataManager.ItemID.COPPER_ARROWHEAD, "s") + " on " + ItemLink(DataManager.ItemID.HEADLESS_ARROW, "s") + ". You can forge " + ItemLink(DataManager.ItemID.COPPER_ARROWHEAD, "s") + " from bars at an anvil."), token);
                await Task.Run(() => Talk(player, npc, token, "Ok, be right back."), token);
                return EXIT;
            }

        case 240:
            await Task.Run(() => Talk(player, npc, token, "I've forgotten what I was supposed to be doing"), token);
            await Task.Run(() => Talk(npc, player, token, "Go and see " + NpcLink(NIKA) + " by the cliffs to the South East, they'll help you find something a bit more protective to wear."), token);
            await Task.Run(() => Talk(player, npc, token, "Great, thanks for your help."), token);
            return EXIT;
    }
    return EXIT;
};

Func<Npc, Player, int, CancellationToken, Task<int>> ArtisanDialog = async (npc, player, s, token) => {
    int QS = GetQuestState(player, QUEST_ID);

    if (QS < 240 || QS > 280) {
        await Task.Run(() => Talk(npc, player, token, "I don't have anything I can help you with right now."), token);
        await Task.Run(() => Talk(player, npc, token, "Ok, thanks."), token);
        return EXIT;
    }
    switch (QS) {
        case 240:
            Prompt(player, npc,"Have you come to learn about artisanry?");
            choice = await Task.Run(() => Response(player, npc, token, "Learn about artisanry?", "Yes", "No"), token);

            switch (choice) {
                case 0:
                    CompleteStep(player, QUEST_ID, 250, 0);
                    AddQuestStep(player, QUEST_ID, 260, "Show a " + ItemLink(DataManager.ItemID.COWHIDE) + " to " + NpcLink(NIKA));
                    await Task.Run(() => Talk(player, npc, token, "I'd love to learn about artisanry!"), token);
                    await Task.Run(() => Talk(npc, player, token, "Great!| Artisans can make armour from leather and cloth to help protect you from enemy attacks."), token);
                    await Task.Run(() => Talk(npc, player, token, "We'll get you started by making some " + ItemLink(DataManager.ItemID.LEATHER_GLOVES)), token);
                    await Task.Run(() => Talk(player, npc, token, "How do I do that?"), token);
                    return 0;
                case 1:
                    await Task.Run(() => Talk(player, npc, token, "Not right now."), token);
                    return EXIT;
            }
            break;

        case 250:
            if (HasInventory(player, DataManager.ItemID.COWHIDE, 1)) {
                CompleteStep(player, QUEST_ID, 260, 0);
                AddQuestStep(player, QUEST_ID, 270, "Show a " + ItemLink(DataManager.ItemID.LEATHER_ROLL) + " to " + NpcLink(NIKA));
                await Task.Run(() => Talk(npc, player, token, "Awesome! Now we can tan the " + ItemLink(DataManager.ItemID.COWHIDE) + " into a " + ItemLink(DataManager.ItemID.LEATHER_ROLL)), token);
                await Task.Run(() => Talk(player, npc, token, "Ok great, how do I do that?"), token);
                return 0;

            } else {
                await Task.Run(() => Talk(npc, player, token, "You'll need animal hide to turn into leather, you can get " + ItemLink(DataManager.ItemID.COWHIDE) + " from killing " + NpcLink(12, "s")), token);
                await Task.Run(() => Talk(player, npc, token, "Ok, be right back."), token);
                return EXIT;
            }

        case 260:
            if (HasInventory(player, DataManager.ItemID.LEATHER_ROLL, 1)) {
                CompleteStep(player, QUEST_ID, 270, 0);
                AddQuestStep(player, QUEST_ID, 280, "Talk to " + NpcLink(NIKA));
                await Task.Run(() => Talk(npc, player, token, "Awesome! Now you can craft some " + ItemLink(DataManager.ItemID.LEATHER_GLOVES)), token);
                await Task.Run(() => Talk(player, npc, token, "Ok great, how do I do that?"), token);
                return 0;

            } else {
                await Task.Run(() => Talk(npc, player, token, "There is a tanning station nearby, you can turn " + ItemLink(DataManager.ItemID.COWHIDE) + " into a " + ItemLink(DataManager.ItemID.LEATHER_ROLL) + " there."), token);
                await Task.Run(() => Talk(player, npc, token, "Ok, be right back."), token);
                return EXIT;
            }

        case 270:
            Prompt(player, npc, "You'll need a " + ItemLink(DataManager.ItemID.NEEDLE) + " and " + ItemLink(DataManager.ItemID.THREAD) + " to make a pair of " + ItemLink(DataManager.ItemID.LEATHER_GLOVES) + ", I have some spare I can give you if you want?");
            choice = await Task.Run(() => Response(player, npc, token, "Take the "+ ItemLink(DataManager.ItemID.NEEDLE) + " and " + ItemLink(DataManager.ItemID.THREAD) + "?", "Yes", "No"), token);

            switch (choice) {
                case 0:
                    await Task.Run(() => Talk(player, npc, token, "Yes please!"), token);
                    if (HasSpace(player, 2)) {
                        if (GiveItem(player, DataManager.ItemID.THREAD)) {
                            if (GiveItem(player, DataManager.ItemID.NEEDLE)) {
                                CompleteStep(player, QUEST_ID, 280, 0);
                                AddQuestStep(player, QUEST_ID, 290, "Show a pair of " + ItemLink(DataManager.ItemID.LEATHER_GLOVES) + " to " + NpcLink(NIKA));
                                return 0;
                            }
                        }
                    }
                    return EXIT_FULL_INV;

                case 1:
                    await Task.Run(() => Talk(player, npc, token, "Not right now."), token);
                    return EXIT;
            }
            break;

        case 280:
            if (HasInventory(player, DataManager.ItemID.LEATHER_GLOVES, 1)) {
                CompleteStep(player, QUEST_ID, 290, 0);
                AddQuestStep(player, QUEST_ID, 300, "Talk to " + NpcLink(JEN));
                await Task.Run(() => Talk(npc, player, token, "Awesome! You've got a fair amount of stuff now, maybe " + NpcLink(JEN) + " could help you store them all somewhere."), token);
                await Task.Run(() => Talk(player, npc, token, "Ok thanks, see you later."), token);
                return EXIT;

            } else {
                await Task.Run(() => Talk(npc, player, token, "Use the " + ItemLink(DataManager.ItemID.NEEDLE) + " in your inventory to craft some " + ItemLink(DataManager.ItemID.LEATHER_GLOVES) + ". Make sure you have a " + ItemLink(DataManager.ItemID.LEATHER_ROLL)), token);
                await Task.Run(() => Talk(player, npc, token, "Ok, be right back."), token);
                return EXIT;
            }
    }
    return EXIT;
};

Func<Npc, Player, int, CancellationToken, Task<int>> ShopkeeperDialog = async (npc, player, s, token) => {
    int QS = GetQuestState(player, QUEST_ID);

    if (QS != 290) {
        await Task.Run(() => Talk(npc, player, token, "I don't have anything I can help you with right now."), token);
        await Task.Run(() => Talk(player, npc, token, "Ok, thanks."), token);
        return EXIT;
    }

    switch (QS) {
        case 290:
            await Task.Run(() => Talk(npc, player, token, "Nice to meet you, I can see you're new around here."), token);
            await Task.Run(() => Talk(npc, player, token, "I help run the local bank. You look like you could do with clearing your inventory."), token);
            await Task.Run(() => Talk(npc, player, token, "You can store all your items securely at banks, and they will be accessible from any bank, any time you want."), token);
            await Task.Run(() => Talk(player, npc, token, "Great thanks, that's really useful."), token);
            await Task.Run(() => Talk(npc, player, token, "No problem. I think you're ready to head out into the world on your own now. Take this to help get you started."), token);
            CompleteStep(player, QUEST_ID, 300, 0);
            GiveStackingItem(player, DataManager.ItemID.COINS, 100);
            await Task.Run(() => Talk(player, npc, token, "Wow, thank you!"), token);
            await Task.Run(() => Talk(npc, player, token, "No worries, see you around."), token);
            return EXIT;
    }
    return EXIT;
};

RegisterNPCQuest(THE_STRANGER, StrangerDialog, QUEST_ID);
RegisterNPCQuest(JACK, JackLumberDialog, QUEST_ID);
RegisterNPCQuest(FRITZ, BillyBassDialog, QUEST_ID);
RegisterNPCQuest(BUCK, DrillPitTaylorDialog, QUEST_ID);
RegisterNPCQuest(CHARSI, SmithDialog, QUEST_ID);
RegisterNPCQuest(MURRAY, SurvivalistDialog, QUEST_ID);
RegisterNPCQuest(NIKA, ArtisanDialog, QUEST_ID);
RegisterNPCQuest(JEN, ShopkeeperDialog, QUEST_ID);

Action<Player> LightFire = (player) => {
    CompleteStep(player, QUEST_ID, 90, 0);
    AddQuestStep(player, QUEST_ID, 100, "Talk to " + NpcLink(THE_STRANGER));
    UnRegisterSpawnObject(40, player);
};

Action<Player> StanceTest = (player) => {
    CompleteStep(player, QUEST_ID, 170, 0);
    AddQuestStep(player, QUEST_ID, 180, "Talk to " + NpcLink(MURRAY));
    UnRegisterStanceChange(1, player);
};
