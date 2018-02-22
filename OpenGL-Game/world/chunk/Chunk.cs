﻿using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;

namespace OpenGL_Game
{
    class Chunk
    {
        private int[,,] chunkBlocks;

        public BlockPos chunkPos { get; }

        public Chunk(BlockPos chunkPos)
        {
            this.chunkPos = chunkPos;

            chunkBlocks = new int[16, 16, 16];
        }

        private Chunk(ChunkCache cache)
        {
            chunkPos = cache.chunkPos;
            chunkBlocks = cache.chunkBlocks;
        }

        public static Chunk CreateFromCache(ChunkCache cache)
        {
            return new Chunk(cache);
        }

        public void setBlock(BlockPos pos, EnumBlock blockType)
        {
            chunkBlocks[pos.x, pos.y, pos.z] = (int)blockType;
        }

        public EnumBlock getBlock(BlockPos pos)
        {
            if (isPosInChunk(pos))
                return (EnumBlock)chunkBlocks[pos.x, pos.y, pos.z];

            var block = Game.INSTANCE.world.getBlock(pos + chunkPos);

            return block;
        }

        private bool isPosInChunk(BlockPos pos)
        {
            return
                pos.x >= 0 && pos.x < chunkBlocks.GetLength(0) &&
                pos.y >= 0 && pos.y < chunkBlocks.GetLength(1) &&
                pos.z >= 0 && pos.z < chunkBlocks.GetLength(2);
        }

        public bool isBlockAbove(BlockPos pos)
        {
            if (isPosInChunk(pos))
            {
                for (int y = pos.y + 1; y < 16; y++) //TODO: KEEP CHECKING ABOVE
                {
                    var bp = new BlockPos(pos.x, y, pos.z);

                    if (getBlock(bp) != EnumBlock.AIR)
                        return true;
                }
            }

            return Game.INSTANCE.world.isBlockAbove(pos + chunkPos);
        }

        public ChunkModel generateModel(ChunkModel previousChunkModel)
        {
            var possibleDirections = (EnumFacing[])Enum.GetValues(typeof(EnumFacing));
            var pos = new BlockPos(0, 0, 0);
            List<RawQuad> quads;

            var l_x = chunkBlocks.GetLength(0);
            var l_y = chunkBlocks.GetLength(1);
            var l_z = chunkBlocks.GetLength(2);

            var MODEL_RAW = new Dictionary<ShaderProgram, List<RawQuad>>();

            //generate the model / fill MODEL_RAW
            for (int z = 0; z < l_z; z++)
            {
                for (int y = 0; y < l_y; y++)
                {
                    for (int x = 0; x < l_x; x++)
                    {
                        pos.setPos(x, y, z);

                        var block = getBlock(pos);

                        if (block == EnumBlock.AIR)
                            continue;

                        var blockModel = ModelManager.getModelForBlock(block);

                        if (!MODEL_RAW.TryGetValue(blockModel.shader, out quads))
                            MODEL_RAW.Add(blockModel.shader, quads = new List<RawQuad>());

                        for (int i = 0; i < possibleDirections.Length; i++)
                        {
                            var dir = possibleDirections[i];

                            if (getBlock(pos.offset(dir)) == EnumBlock.AIR)
                            {
                                /*if (isBlockAbove(pos)) //TODO: Lighting
                                {

                                }*/

                                var quad = ((RawBlockModel)blockModel.rawModel)?.getQuadForSide(dir)?.offset(pos);

                                if (quad != null)
                                    quads.Add(quad);
                            }
                        }
                    }
                }
            }

            var previousShaders = previousChunkModel.getShadersPresent();

            var finish = new ThreadLock(() =>
            {
                var newShaders = MODEL_RAW.Keys.ToArray();

                for (int i = 0; i < previousShaders.Length; i++)
                {
                    var oldShader = previousShaders[i];

                    if (!newShaders.Contains(oldShader))
                    {
                        previousChunkModel.getFragmentModelWithShader(oldShader, out var oldChunkFragmentModel);
                        //TODO try running only this line on main thread
                        oldChunkFragmentModel.overrideData(new List<RawQuad>());
                    }
                }
            /*});
            Game.MAIN_THREAD_QUEUE.Add(finish);
            finish.WaitFor();

            finish = new ThreadLock(() =>
            {*/
                foreach (var value in MODEL_RAW)
                {
                    var newShader = value.Key;
                    var newData = value.Value;

                    if (!previousShaders.Contains(newShader))
                    {
                        var newFragment = new ChunkFragmentModel(newShader, newData);
                        previousChunkModel.setFragmentModelWithShader(newShader, newFragment);
                    }
                    else
                    {
                        previousChunkModel.getFragmentModelWithShader(newShader, out var oldFragment);

                        oldFragment.overrideData(newData);
                    }
                }
            });
            Game.MAIN_THREAD_QUEUE.Add(finish);
            finish.WaitFor();

            return previousChunkModel;
        }

        public ChunkCache createChunkCache()
        {
            return new ChunkCache(chunkPos, chunkBlocks);
        }
    }

    [Serializable]
    class ChunkCache
    {
        private readonly BlockPos _chunkPos;
        private readonly int[,,] _chunkBlocks;

        public BlockPos chunkPos => _chunkPos;
        public int[,,] chunkBlocks => _chunkBlocks;

        public ChunkCache(BlockPos chunkPos, int[,,] chunkBlocks)
        {
            _chunkPos = chunkPos;
            _chunkBlocks = chunkBlocks;
        }
    }
}