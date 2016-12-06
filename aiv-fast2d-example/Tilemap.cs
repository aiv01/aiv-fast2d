using System;
using System.Collections.Generic;
using System.IO;
using Aiv.Fast2D;
using OpenTK;

namespace Aiv.Fast2D.Example
{
    public class Tilemap
    {
        private Texture tileSheet;
        private Mesh mapMesh;
        private int[] map;

        private int width;
        private int height;

        public Vector2 position
        {
            get
            {
                return this.mapMesh.position;
            }
            set
            {
                this.mapMesh.position = value;
            }
        }


        public Tilemap(string csvFile, string textureName)
        {
            string mapBody = File.ReadAllText(csvFile).TrimEnd(new char[] { '\n' });
            string[] rows = mapBody.Split('\n');
            int index = 0;
            height = rows.Length;
            foreach (string row in rows)
            {
                string[] cols = row.Split(',');
                width = cols.Length;
                if (this.map == null)
                {
                    this.map = new int[cols.Length * rows.Length];
                }
                foreach (string col in cols)
                {
                    this.map[index] = int.Parse(col);
                    index++;
                }
            }

            this.mapMesh = new Mesh();
            List<float> vertices = new List<float>();
            for (int y = 0; y < height; y++)
            {
                for (int x = 0; x < width; x++)
                {
                    vertices.Add(x * 70);
                    vertices.Add(y * 70);

                    vertices.Add(x * 70);
                    vertices.Add((y + 1) * 70);

                    vertices.Add((x + 1) * 70);
                    vertices.Add(y * 70);

                    vertices.Add((x + 1) * 70);
                    vertices.Add(y * 70);

                    vertices.Add((x + 1) * 70);
                    vertices.Add((y + 1) * 70);

                    vertices.Add(x * 70);
                    vertices.Add((y + 1) * 70);
                }
            }

            this.mapMesh.v = vertices.ToArray();
            this.mapMesh.uv = new float[this.mapMesh.v.Length];
            this.mapMesh.Update();
            this.tileSheet = new Texture(textureName);
            // use nearest mode for sampling
            this.tileSheet.SetNearest();
        }

        private int GetTile(int x, int y)
        {
            int index = (y * this.width) + x;
            return this.map[index];
        }

        private Vector2 GetTileXY(int index)
        {
            int x = (index % 12) * 71;
            int y = (index / 12) * 71;

            return new Vector2(x, y);
        }

        public void Draw()
        {
            for (int y = 0; y < height; y++)
            {
                for (int x = 0; x < width; x++)
                {
                    int tileId = GetTile(x, y);
                    if (tileId < 0)
                    {
                        continue;
                    }
                    Vector2 tileXY = GetTileXY(tileId);

                    int textureWidth = this.tileSheet.Width;
                    int textureHeight = this.tileSheet.Height;

                    float deltaW = 1f / textureWidth;
                    float deltaH = 1f / textureHeight;
                    float left = tileXY.X * deltaW;
                    float right = (tileXY.X + 70) * deltaW;
                    float top = tileXY.Y * deltaH;
                    float bottom = (tileXY.Y + 70) * deltaH;

                    int index = (y * width * 12) + (x * 12);

                    this.mapMesh.uv[index] = left;
                    this.mapMesh.uv[index + 1] = top;

                    this.mapMesh.uv[index + 2] = left;
                    this.mapMesh.uv[index + 3] = bottom;

                    this.mapMesh.uv[index + 4] = right;
                    this.mapMesh.uv[index + 5] = top;

                    this.mapMesh.uv[index + 6] = right;
                    this.mapMesh.uv[index + 7] = top;

                    this.mapMesh.uv[index + 8] = right;
                    this.mapMesh.uv[index + 9] = bottom;

                    this.mapMesh.uv[index + 10] = left;
                    this.mapMesh.uv[index + 11] = bottom;
                }
            }
            this.mapMesh.UpdateUV();

            this.mapMesh.DrawTexture(this.tileSheet);
        }


    }
}
