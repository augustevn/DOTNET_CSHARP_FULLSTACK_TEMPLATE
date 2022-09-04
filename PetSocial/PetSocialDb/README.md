## Database

Create migration:

`dotnet ef migrations add InitialMigration --project Libs/PetSocialDb --startup-project PetSocial/PetSocialApi`

Apply migrations:

`dotnet ef database update --project Libs/PetSocialDb --startup-project PetSocial/PetSocialApi`

Remove migrations:

`dotnet ef migrations remove --project Libs/PetSocialDb --startup-project PetSocial/PetSocialApi`

Drop database:

`dotnet ef database drop -f --project Libs/PetSocialDb --startup-project PetSocial/PetSocialApi`