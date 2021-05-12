using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Com.MyCompany.Pacman
{
    public static class Extra
    {
        public static Vector2[] directions = new Vector2[]{
        Vector2.up,
        Vector2.right,
        Vector2.down,
        Vector2.left
    };
        public static Vector2 PerpendicularRight(Vector2 v)
        {
            for (int i = 0; i < directions.Length; i++)
            {
                Vector2 dir = directions[i];
                if (dir == v)
                {
                    int nextDir = i + 1;
                    if (nextDir == directions.Length)
                    {
                        nextDir = 0;
                    }
                    return directions[nextDir];
                }
            }
            throw new KeyNotFoundException("Vector " + v + " is not a horizontal/vertical vector.");
        }
        public static Vector2 PerpendicularLeft(Vector2 v)
        {

            for (int i = 0; i < directions.Length; i++)
            {
                Vector2 dir = directions[i];
                if (dir == v)
                {
                    int nextDir = i - 1;
                    if (nextDir < 0)
                    {
                        nextDir = directions.Length - 1;
                    }
                    return directions[nextDir];
                }
            }
            throw new KeyNotFoundException("Vector " + v + " is not a horizontal/vertical vector.");
        }
    }
}


/* =============================================================================
#  Author:          Divya Gandhi - https://github.com/divya16-bit
#  Email:           16gandhi.hemani@gmail.com
#  FileName:        extra.cs
#  Created On:      26/11/2020
============================================================================= */
