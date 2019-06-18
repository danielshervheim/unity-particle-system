# Particle System in Unity

A GPU accelerated particle system I made for a class assignment.

![Fire](https://imgur.com/GJEple9.png)

![Water](https://imgur.com/yqEsD54.png)

Watch it on [Youtube](https://www.youtube.com/playlist?list=PLI8z64x91TSuIT6wMehc56uLu8R5vshfJ).

### About

Supports the following features:

- Custom emitter shapes (rectangle, circle, sphere, and cone).
- Custom spawn rate (X particles per Y seconds).
- Change in appearance over time (albedo, emission, size, transparency, and speed).
- Real-time gravity vector (can be used to simulate wind).
- Real-time coefficient of restitution (also includes a randomize factor to help hide the perfect “steppes” that arise when every particle has the same COR).
- Static and dynamic box and sphere colliders.

The particle system is initialized on the CPU. I calculate starting positions and velocities for each particle, then upload them to the GPU in buffers where a Compute shader advances the simulation each frame. Each particle is rendered as a quad via GPU instancing and a custom shader.

Because each particle lives on the GPU, my system spawns the maximum number of particles initially. Every particle starts with a “spawn timer” which ticks down. The particle will not appear until the timer has run out. Once a particle dies, it returns to its original position and velocity. This gives the appearance of spawning particles over time, while removing the need for costly CPU-GPU communication every frame to instantiate and destroy particles. 

Sphere and box colliders are also uploaded to the GPU (each frame, at a slightly increased computational cost, or on initialization). This enables the particles to react to the environment (this is demonstrated in the fountain demo).

The coefficient of restitution (which controls the particles “bounciness”) and the gravity vector are updated each frame as well. This allows for complex wind-like behavior.

### Installation

- Clone this repository, and copy the assets folder.

- Create a new project in Unity, and close the editor.

- Replace the project's `Assets` folder with the one from this repository.

- Reopen the project in the editor.

- *Open the Asset Store and download the [Post Processing Stack](https://assetstore.unity.com/packages/essentials/post-processing-stack-83912) to use the included post processing profiles.*

### Setup

- Create a new `scene` in the editor.

- Add an empty `gameObject`.

- Add the `Emitter.cs` script to the empty `gameObject`.

- Setup the `Emitter` as desired, and press play to see the particle system in action.