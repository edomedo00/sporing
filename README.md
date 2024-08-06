# Sporing

Sporing is a game developed in [Unity](https://unity.com/). The music and audio were designed, created, and run through [SuperCollider](https://supercollider.github.io/). The connection between Unity and SuperCollider is established through the OSC protocol: Unity sends messages using the [extOSC](https://github.com/Iam1337/extOSC) library to SuperCollider, which responds to those messages by generating audio or modifying its parameters in real-time.
