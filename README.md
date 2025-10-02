# MakeYourBulk

This mod was published on Steam Workshop (17-12-2024) and now on GitHub (04-04-2025) for debugging purposes and for the sake of compatibility with all mods.

I recommend you, before going to the code, to at least read what I have to say:

* This mod has the purpose of creating a DIY bulk recipe, aka a recipe that already exists in the game or in a mod and changes its products, work amount, or cost. 

* The code follows this journey:

  1. After "LongEventHandler.ExecuteWhenFinished(...)", it goes into "MakeYourBulkSettings.AddToDatabase()".
  2. In here the bulk recipe creates itself with "BulkRecipe.CreateBulkRecipeDef(...)".
  3. Then it controls if the recipe is good, and then it is added into "DefDatabase<RecipeDef>" and its "recipeUsers".
  4. After the game loads into the menu for the first time, the user can create other bulk recipes with the logic found in the class "Dialog_AddRecipe".
  5. If the user adds a new recipe, it also goes into "MakeYourBulkSettings.AddToDatabase()".
