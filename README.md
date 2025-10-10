# MakeYourBulk

This mod was published on Steam Workshop (17-12-2024) and now on GitHub (04-04-2025) for debugging purposes and for the sake of compatibility with all mods.

I recommend you, before going to the code, to at least read what I have to say:

* This mod has the purpose of creating a DIY bulk recipe, aka a recipe that already exists in the game or in a mod and changes its products, work amount, or cost. 

* The code that creates the new RecipeDef follows this journey:
  1. MakeYourBulkMod.cs: In the constructor "MakeYourBulkMod(ModContentPack content)" it inits the settings and LongEventHandler.ExecuteWhenFinished(BulkRecipeGenerator.LoadBulkRecipeDefs).
  2. BulkRecipeGenerator.LoadBulkRecipeDefs gets called and loops through every BulkRecipe created from the user.
  3. In each loop, after it is asserted that it has a BaseRecipeDef and that it isn't already loaded, it creates the new RecipeDef with BulkRecipeGenerator.CreateBulkRecipeDef(...).
  4. Then after it creates the new RecipeDef (we still are in the loop) it gets added into DefDatabase<RecipeDef> and into missings recipe users based on the BaseRecipeDef.
  5. After the loop ended, DefDatabase<RecipeDef>.ResolveAllReferences() gets called.

* Everytime the user changes in the settings (MakeYourBulkMod.cs: MakeYourBulkMod.WriteSettings()), every BulkRecipe rebuilds its Work Amount and Cost (so that if the user changed its properties, they get applied). Even if the properties haven't changed, this still happens. 
P.S. This also happens in MakeYourBulkSettings.cs: MakeYourBulkSettings.ShowRecipeEntryWorkAmount() & MakeYourBulkSettings.ShowRecipeEntryCost() to show the updated tooltips.

* If you have any problem or you can give me any help with something you think it doesn't quite "work as intended", don't hesitate to contact me. 
