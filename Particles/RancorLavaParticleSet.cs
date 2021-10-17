using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System.Collections.Generic;
using System.Linq;
using Terraria;
using Terraria.Graphics.Shaders;
using Terraria.ModLoader;

namespace CalamityMod.Particles
{
	public class RancorLavaParticleSet : BaseFusableParticleSet
	{
		public override float BorderSize => 18f;
		public override bool BorderShouldBeSolid => false;
		public override Color BorderColor => Color.Lerp(Color.Yellow, Color.Red, 0.85f) * 0.85f;
		public override FusableParticleRenderLayer RenderLayer => FusableParticleRenderLayer.OverNPCsBeforeProjectiles;

		private static readonly List<Texture2D> _backgroundTextures = new List<Texture2D>()
		{
			ModContent.GetTexture("CalamityMod/Projectiles/InvisibleProj"),
		};
		public override List<Effect> BackgroundShaders => new List<Effect>()
		{
			GameShaders.Misc["CalamityMod:AdditiveFusableParticleEdge"].Shader,
		};
		public override List<Texture2D> BackgroundTextures => _backgroundTextures;
		public override FusableParticle SpawnParticle(Vector2 center, float sizeStrength)
		{
			Particles.Add(new FusableParticle(center, sizeStrength));
			return Particles.Last();
		}

		public override void UpdateBehavior(FusableParticle particle)
		{
			particle.Size = MathHelper.Clamp(particle.Size - 0.3f, 0f, 200f) * 0.99f;
			if (particle.Size < 25f)
				particle.Size = particle.Size * 0.95f - 0.9f;
		}

		public override void DrawParticles()
		{
			Texture2D fusableParticleBase = ModContent.GetTexture("CalamityMod/ExtraTextures/FusableParticleBase");
			foreach (FusableParticle particle in Particles)
			{
				Vector2 drawPosition = particle.Center - Main.screenPosition;
				Vector2 origin = fusableParticleBase.Size() * 0.5f;
				Vector2 scale = Vector2.One * particle.Size / fusableParticleBase.Size();
				Main.spriteBatch.Draw(fusableParticleBase, drawPosition, null, BorderColor * 1.4f, 0f, origin, scale, SpriteEffects.None, 0f);
			}
		}
	}
}
