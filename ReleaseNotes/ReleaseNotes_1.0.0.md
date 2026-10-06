# Release Notes 

## Version
- Version: `1.0.0`

Deprecated versioning number! Now working with semver in frontend package.json. 

## Release Date
- Date: `28-01-2026`

## Features
This release marks the final version of the first stage of the product and contains the following features:

### Site Navigation
- **Landing page** - entry point of application
- **Home page** - default destination after login

### User management
- **Registration form** - allows new users to sign up and create an account.
- **Login page** - allows existing users to sign in.
- **User management dashboard** - view, edit and delete existing users.
- **Question management dashboard** - view, edit and deactivate existing questions.
- **Sign out button** - signs out the user and redirects to login screen.
- **User grouping** - group users and have their performance data shown.

### Cosmetics
- **Currency** - can be obtained from tests and used on cosmetics.
- **Customization page** - allows users to spend the currency on cosmetics in the shop and allows users to equip them in the inventory page.

### Items
- **Database populator** - populates the items database with the ShareStats Item Bank items.
- **Item management dashboard** - view, edit and deactivate items in the database.
- **AI item generation interface** - generate an item using an OpenAI model. 
- **Report option** - users can report an item when the item appears to be invalid or incomplete.

### Levels
- **QTI Player** - environment developed by CitoLab where tests can be taken. This environment has been incorporated into the application.
- **Random level generation** - automatically builds a level using random items pulled from the database. There is full control over level size and included item domains via the QTI window.
- **Currency rewards** - when getting questions correct, you will gain the customization currency to spend in the shop.

### Proficiencies
- **Randomized start values** - are assigned to users on account creation.
- **Proficiency changes** - when getting questions right or wrong.
- **Domain options for questions** - are determined by the Domain Dependency and the user's proficiency in certain domains.

### Settings
- **Global settings** - can be made in the admin version of the program that apply to the entire program.
- **Global setting profiles** - can be set up so it takes only one click to switch between settings.

## Known Issues
This release contains the following known issues/bugs:

### User management
- **Grouping users works with userIds** - The user dashboard does not show userIds, so grouping requires database access or lucky guessing.
- **User dashboard current user account deletion** - users can delete their own account in the admin dashboard.

### Items
- **Database populator requires manual run** - Not done automatically.
- **Automatic generation of questions is disabled** - As there is currently no scheduler made yet, automatic creation of questions to fill a backlog (to make sure every user always has at least X questions available for all domains they have access to) is not useful to have enabled. Attempting to prompt the AI API for 350 questions of 8000 tokes without any delay when the maximum is 250 000 tokens per minute resulted in token rate limits.

### Levels
- **Correct answer visible with inspect element** - the QTI player has a bug where users are able to view the correct answer of an item using inspect element.
- **Deactivated items can appear in levels** - Deactivated items can be selected for levels.

### Settings
- **Only the LevelSize setting is currently functional** - the UI for settings is slightly strange with frontend input and only one of the two settings is functional.

### Frontend
- **API calls are often executed twice** - Because in `frontend/src/main.tsx` we use StrictMode for our entire frontend app (to be able to debug it), API calls are executed twice.

### Other
- **Home page does not show user data** - Not implemented yet.
- **Not all features and files are fully tested** - Some features were created without corresponding unit tests due to time limitations.