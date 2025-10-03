# MakeYourBulk

This mod was published on Steam Workshop (17-12-2024) and now on GitHub (04-04-2025) for debugging purposes and for the sake of compatibility with all mods.

I recommend you, before going to the code, to at least read what I have to say:

* This mod has the purpose of creating a DIY bulk recipe, aka a recipe that already exists in the game or in a mod and changes its products, work amount, or cost. 

* The code that creates the new RecipeDef follows this journey:
  1. [StaticConstructorOnStartup] MakeYourBulkMod.cs: static LoadBulkRecipeDefs()
  2. Reads the settings.
  3. Loops every Bulk Recipe created from the user.
  4. In each loop, after it is asserted that it has a BaseRecipeDef and that it isn't already loaded, BulkRecipe.cs: BulkRecipe::GetBulkRecipeDef(...) gets called and creates the new RecipeDef.
  5. Then after it creates the new RecipeDef (we still are in the loop) it gets added into DefDatabase<RecipeDef> and, if needed, into missings recipe users based on the BaseRecipeDef.
  6. After the loop ended, DefDatabase<RecipeDef>.ResolveAllReferences() gets called.

* If you have any problem or you can give me any help with something you think it doesn't quite "work as intended", don't hesitate to contact me. 
