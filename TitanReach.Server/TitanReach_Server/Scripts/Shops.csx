using System;
using System.Threading.Tasks;
using System.Collections.Generic;
using TitanReach_Server;

static int FishingShop = 0;
static int GeneralStore = 1;
static int AlchemyStore = 2;
static int SwordStore = 3;
static int RareStore = 4;
static int VegStore = 6;
static int FishingShop2 = 7;

//General shops
RegisterNPCShop(107, (player) => OpenShop(GeneralStore, player), GeneralStore);
RegisterNPCShop(101, (player) => OpenShop(GeneralStore, player), GeneralStore);
RegisterNPCShop(105, (player) => OpenShop(RareStore, player), RareStore);
RegisterNPCShop(106, (player) => OpenShop(SwordStore, player), SwordStore);
RegisterNPCShop(108, (player) => OpenShop(VegStore, player), VegStore);
RegisterNPCShop(95, (player) => OpenShop(FishingShop, player), FishingShop);
RegisterNPCShop(118, (player) => OpenShop(FishingShop2, player), FishingShop2);
