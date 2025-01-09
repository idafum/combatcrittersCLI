# UserProfilesConsoleApp

This .NET console application interacts with user profiles

## Prerequisites

- Docker installed on your machine. Ensure docker is running.

## Running the Application with Docker

1. Pull the Docker Image

```
docker pull idafum/userprofilescli:1.0
```

2. Run the Docker Container

```
docker run --rm -it idafum/userprofilescli:1.0

```

## Feature: [User Profiles](https://github.com/orgs/InternetEnemies/projects/1/views/1?filterQuery=label%3A%22A+-+Feature%22&pane=issue&itemId=80276330&issue=InternetEnemies%7CCombatCritters%7C6)

### User Stories

1. As a User, I want to be able to create a profile.
   - Register and Login UI
2. As a admin User, I want to browse and edit all our User Profile
   - View all Users and Delete a User
3. As a User I want to be able to add friends and accept a friend request
   - Send and accept friend request
4. **Not Implemented**As a User, I want to feature decks on my profile
   - Choose from deck list to feature a deck on profile.

## Reflection

The API documentation was extremely helpful. This CLI was easy to add because of the n-tier clear separation of concerns.
It allowed modification in one layer without affecting the others.
