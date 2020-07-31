using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Aiv.Fast2D;
using OpenTK;

namespace Aiv.Fast2D.Example.PRE
{
    public class Particles
    {
        private struct Particle
        {
            public Vector2 velocity;
            public Vector2 gravity;
            public float life;
            public float speed;
        }

        private InstancedSprite instancedSprite;

        private Particle[] particles;

        private Random random;

        private Vector2 gravity = new Vector2(0, 20f);

        public Vector2 position
        {
            get
            {
                return this.instancedSprite.position;
            }
            set
            {
                this.instancedSprite.position = value;
            }
        }

        public float RandomFloat(float min, float max)
        {
            return (float)(random.NextDouble() * (max - min) + min);
        }

        public Particles(float width, float height, int numberOfParticles)
        {
            this.random = new Random();
            this.instancedSprite = new InstancedSprite(width, height, numberOfParticles);
            this.particles = new Particle[numberOfParticles];
            for(int i=0;i<numberOfParticles;i++)
            {
                this.particles[i].speed = RandomFloat(30, 60);
                this.particles[i].velocity = new Vector2(RandomFloat(-1, 1), RandomFloat(-1, 0)) * this.particles[i].speed;
                this.particles[i].life = RandomFloat(1, 3);
                this.instancedSprite.SetScale(i, new Vector2(RandomFloat(1, 2), RandomFloat(1, 2)), uploadImmediatly: true);
            }
            this.instancedSprite.UpdateScaleForAllInstances();
        }

        public void DrawColor(int r, int g, int b, int a, float deltaTime)
        {
            Update(deltaTime);
            this.instancedSprite.DrawColor(r, g, b, a);
        }

        public void DrawTexture(Texture texture, float deltaTime)
        {
            Update(deltaTime);
            this.instancedSprite.DrawTexture(texture);
        }

        private void Update(float deltaTime)
        {
            for (int i = 0; i < this.instancedSprite.Instances; i++)
            {
                this.particles[i].life -= deltaTime;
                if (this.particles[i].life <= 0)
                {
                    this.particles[i].velocity = new Vector2(RandomFloat(-1, 1), RandomFloat(-1, 0)) * 60f;
                    this.particles[i].gravity = Vector2.Zero;
                    this.particles[i].life = RandomFloat(1, 3);
                    this.instancedSprite.SetPositionPerInstance(i, Vector2.Zero);
                    this.instancedSprite.SetScale(i, new Vector2(RandomFloat(1, 2), RandomFloat(1, 2)));
                    continue;
                }
                Vector2 position = this.instancedSprite.GetPositionPerInstance(i);
                this.particles[i].gravity += gravity * deltaTime;
                this.instancedSprite.SetPositionPerInstance(i, position + (this.particles[i].velocity + this.particles[i].gravity) * deltaTime);
                if (i % 2 == 0) instancedSprite.SetAdditiveTintPerInstance(i, new Vector4(0, 1, 1, 1));
            }
            this.instancedSprite.UpdatePositionForAllInstances();
            this.instancedSprite.UpdateAdditiveTintForAllInstances();
        }
    }
}
