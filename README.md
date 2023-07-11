<h1 align="center">
<span>Space Arena Party</span>
</h1>
<p align="center">
    English | <a href="./README-CN.md">中文</a>
</p>

Space Arena Party is a multiplayer social game demo that integrates the "Friends" service, "Social Interaction" service, and "Room & Matchmaking" service from the PICO platform. You can experience the following features in the demo:

- Avatar and character movement
- Creating and joining virtual rooms
- Real-time multiplayer interaction
- Friend system, including viewing friend list and inviting friends

![image](https://github.com/Pico-Developer/PlatformSamples-Unity-SpaceArenaParty/assets/110143438/d2239b63-e0e7-4a67-9d2b-0cdb3bde2c3f)<h1 align="center">

## Requirement

Unity Editor's version should be 2021.3.22. Unity 2022 is currently not supported.

## Preview video

You can play the following short video to view the demo's visual design and what you can do with it.

https://p9-arcosite.byteimg.com/obj/tos-cn-i-goo7wpa0wc/8dca694b2ba1435e9488902e407290e8

## Main modules

The demo is made up of the following modules:

| Module | Description |
|---|---|
| Avatar | Avatar is a user's virtual representation. In this demo, the avatar is able to perform different poses, such as raising the arm and turning around, based on the data input from the HMD and controllers. Additionally, you can change the color of the avatar. |
| Virtual Room | The demo provides three rooms: lobby, blue room, and orange room. The three rooms are only different in colors. The lobby is a private room that users join by default after launching the game. Users can join and create other rooms using the launch pad. |
| Launch Pad | Users can create rooms, join rooms, and check out their friend lists using the launch pad. |
| Multiplayer | Users can get multiplayer experience, including inviting friends to their current rooms, joining their friends, and more. |
| Social System | The demo implements the SDK's Friends service. After login, users can view their friend lists, check if their friends are online, and invite friends via the launch pad. |

## Complete procedure

Below is the overall procedure for experiencing the demo:
1. Pull the project.
2. Create a PICO Developer account, organization, and app.
3. Enable the matchmaking service and create a matchmaking pool.
4. Create destinations.
5. Set the app ID.
6. Complete PC-end debugging settings.
7. Run the demo in the Unity Editor or on the PICO device.

## Debug the multiplayer capability

Below are the methods you can use to debug the multiplayer capability:
- Use the Unity Editor and a PICO device
- Use multiple PICO devices

## Learn more

For detailed instructions on the procedure for experiencing the demo, more information on multiplayer capability debugging, etc., refer to the "[Social Interaction: Demo](https://developer-global.pico-interactive.com/document/unity/social-interaction-demo/)" article.

## Contributing
We welcome contributions to the project! If you would like to contribute, please follow the steps below:
1. Fork the repository.
2. Create a new branch for your feature or bugfix.
3. Commit your changes to the new branch.
4. Create a merge request and provide a detailed description of your changes.
