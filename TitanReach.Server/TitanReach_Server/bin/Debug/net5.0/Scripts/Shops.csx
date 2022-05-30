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
RegisterNPCShop(47, (player) => OpenShop(GeneralStore, player), GeneralStore);
RegisterNPCShop(23, (player) => OpenShop(GeneralStore, player), GeneralStore);
RegisterNPCShop(45, (player) => OpenShop(RareStore, player), RareStore);
RegisterNPCShop(46, (player) => OpenShop(SwordStore, player), SwordStore);
RegisterNPCShop(48, (player) => OpenShop(VegStore, player), VegStore);
RegisterNPCShop(18, (player) => OpenShop(FishingShop, player), FishingShop);
RegisterNPCShop(58, (player) => OpenShop(FishingShop2, player), FishingShop2);
