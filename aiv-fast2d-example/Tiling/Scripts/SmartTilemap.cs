using System;
using System.Collections.Generic;
using System.IO;
using Aiv.Fast2D;
using OpenTK;

namespace Aiv.Fast2D.Example.TLE
{
    public class SmartTilemap
    {
        private Texture tileSheet;
        private Mesh mapMesh;
        private int[] map;

        private int width;
        private int height;
        private int tileSize;
        private int tileSizeIncludingExtraBorder;
        private int tilesPerRow;
        private int tilesPerCol;

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


        public SmartTilemap(string csvFile, string textureName, int tileSize, bool extraRightBottomBorderPerTile = false)
        {
            //TEXTURE DATA
            this.tileSheet = new Texture(textureName);
            // use nearest mode for sampling
            this.tileSheet.SetNearest();
            this.tileSize = tileSize;
            
            //Eventual extra border on right and bottom side of each tile. This is just a scenario of how spritesheet can be stored on a texture.
            this.tileSizeIncludingExtraBorder = tileSize;
            if (extraRightBottomBorderPerTile) tileSizeIncludingExtraBorder += 1;

            this.tilesPerRow = tileSheet.Width / tileSizeIncludingExtraBorder;
            this.tilesPerCol = tileSheet.Height / tileSizeIncludingExtraBorder;
            if (tilesPerRow * tileSizeIncludingExtraBorder != tileSheet.Width) throw new Exception("Invalid tilesheet Width! Should include extra 1px border (right and bottom) for each tile");
            if (tilesPerCol * tileSizeIncludingExtraBorder != tileSheet.Height) throw new Exception("Invalid tilesheet Height! Should include extra 1px border (right and bottom) for each tile");


            //PARSE TILE SCENE
            string mapBody = File.ReadAllText(csvFile);
            mapBody = mapBody.TrimEnd(new char[] { '\n', '\r' }).TrimEnd();
            mapBody = mapBody.Replace("\r\n", "\n");

            string[] rows = mapBody.Split('\n');
            int index = 0;
            height = rows.Length;
            foreach (string row in rows)
            {
                string[] cols = row.Split(',');
                width = cols.Length;
                if (this.map == null)
                {
                    this.map = new int[width * height];
                }
                foreach (string col in cols)
                {
                    this.map[index] = int.Parse(col);
                    index++;
                }
            }

            mapMesh = new Mesh();
            mapMesh.v = ComputeVertices();
            mapMesh.uv = ComputeUvs();
            mapMesh.Update();
        }

        private float[] ComputeVertices()
        {
            List<float> vertices = new List<float>();
            for (int y = 0; y < height; y++)
            {
                for (int x = 0; x < width; x++)
                {
                    vertices.Add(x * tileSize);
                    vertices.Add(y * tileSize);

                    vertices.Add(x * tileSize);
                    vertices.Add((y + 1) * tileSize);

                    vertices.Add((x + 1) * tileSize);
                    vertices.Add(y * tileSize);

                    vertices.Add((x + 1) * tileSize);
                    vertices.Add(y * tileSize);

                    vertices.Add((x + 1) * tileSize);
                    vertices.Add((y + 1) * tileSize);

                    vertices.Add(x * tileSize);
                    vertices.Add((y + 1) * tileSize);
                }
            }

            return vertices.ToArray();
        }

        private float[] ComputeUvs()
        {
            float[] uvs = new float[height * width * 12]; // h * w * 12 = num of vertices

            for (int y = 0; y < height; y++)
            {
                for (int x = 0; x < width; x++)
                {
                    int tileId = GetTile(x, y);
                    if (tileId < 0)
                    {
                        continue;
                        // For tile -1 will happen that UVs will point to (0,0) cordinate of the texture
                        // so the whole tile on the screen will be filled with texture(0, 0)
                    }

                    Vector2 tileXY = GetTileXY(tileId);
                    int textureWidth = this.tileSheet.Width;
                    int textureHeight = this.tileSheet.Height;

                    float deltaW = 1f / textureWidth;
                    float deltaH = 1f / textureHeight;
                    float left = tileXY.X * deltaW;
                    float right = (tileXY.X + tileSize) * deltaW;
                    float top = tileXY.Y * deltaH;
                    float bottom = (tileXY.Y + tileSize) * deltaH;

                    int index = (y * width * 12) + (x * 12); //12 is stride (6 vertices x 2 coordinates)

                    uvs[index + 0] = left;
                    uvs[index + 1] = top;

                    uvs[index + 2] = left;
                    uvs[index + 3] = bottom;

                    uvs[index + 4] = right;
                    uvs[index + 5] = top;

                    uvs[index + 6] = right;
                    uvs[index + 7] = top;

                    uvs[index + 8] = right;
                    uvs[index + 9] = bottom;

                    uvs[index + 10] = left;
                    uvs[index + 11] = bottom;
                }
            }
            return uvs;
        }

        private int GetTile(int x, int y)
        {
            int index = (y * this.width) + x;
            return this.map[index];
        }

        private Vector2 GetTileXY(int index)
        {
            int x = (index % tilesPerRow) * tileSizeIncludingExtraBorder;
            int y = (index / tilesPerRow) * tileSizeIncludingExtraBorder;

            return new Vector2(x, y);
        }

        public void Draw()
        {
            this.mapMesh.DrawTexture(this.tileSheet);
        }


    }
}
