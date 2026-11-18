# Unity 2D Exercises - Quizzes 2 and 8

This Unity 6000.3.22f1 project contains a single playable scene for the two practical assignments.

## Run

1. Open this folder as a Unity project.
2. Open `Assets/Scenes/QuizExercises.unity`.
3. Press Play.
4. Use A/D or the arrow keys to move, Space to jump, and R to restart.

The scene is built at runtime by `ExerciseBootstrap`. The character uses a sprite from the ELV Games *Free TopDown RPG Retro Sprites!* pack. The original license is in `Assets/Resources/ELV/License.txt`.

## Assignment coverage

- Quiz 2: the character uses player input and `Rigidbody2D` physics for horizontal movement and jumping.
- Quiz 8: the orange platform has `BoxCollider2D` and `Rigidbody2D`. It starts motionless with no gravity. The first physical contact with the player starts horizontal movement at a constant speed. After exactly five seconds of fixed physics steps, horizontal movement stops, gravity is enabled, and later contacts cannot restart it.

No Unity build is required by either practical quiz; submit this project as a ZIP or provide a public repository link.
