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
            /* 
             * extra border on right and bottom side of each tile.
             * The extra border is useful so when Texture Filtering with GL_NEAREST
             * is applyied, there is more chance the right texel is selected (avoiding "lines" in between tiles)
             */
            this.tileSizeIncludingExtraBorder = tileSize;
            if (extraRightBottomBorderPerTile) tileSizeIncludingExtraBorder += 1;

            this.tilesPerRow = tileSheet.Width / tileSizeIncludingExtraBorder;
            this.tilesPerCol = tileSheet.Height / tileSizeIncludingExtraBorder;
            if (tilesPerRow * tileSizeIncludingExtraBorder != tileSheet.Width) throw new Exception("Invalid tilesheet Width! Should include extra 1px border (right and bottom) for each tile");
            if (tilesPerCol * tileSizeIncludingExtraBorder != tileSheet.Height) throw new Exception("Invalid tilesheet Height! Should include extra 1px border (right and bottom) for each tile");


            //TILE CONFIGURATION
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

            this.mapMesh.v = vertices.ToArray();
            this.mapMesh.uv = new float[this.mapMesh.v.Length];
            this.mapMesh.Update();
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

                    this.mapMesh.uv[index + 0] = left;
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
