## Database

Create migration:

`dotnet ef migrations add InitialMigration --project PetSocial/PetSocialDb --startup-project PetSocial/PetSocialApi`

Apply migrations:

`dotnet ef database update --project PetSocial/PetSocialDb --startup-project PetSocial/PetSocialApi`

Remove migrations:

`dotnet ef migrations remove --project PetSocial/PetSocialDb --startup-project PetSocial/PetSocialApi`

Drop database:

`dotnet ef database drop -f --project PetSocial/PetSocialDb --startup-project PetSocial/PetSocialApi`