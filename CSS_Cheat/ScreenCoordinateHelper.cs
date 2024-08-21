using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CSS_Cheat
{
    public static class ScreenCoordinateHelper
    {
        public struct Vector3
        {
            public float X;
            public float Y;
            public float Z;

            public Vector3(float x, float y, float z)
            {
                X = x;
                Y = y;
                Z = z;
            }
        }

        public struct Vector2
        {
            public float X;
            public float Y;

            public Vector2(float x, float y)
            {
                X = x;
                Y = y;
            }
        }

        public static Vector2 WorldToScreen(Vector3 playerPos, Vector3 enemyPos, float mouseAngleX, float mouseAngleY, float fov, float screenWidth, float screenHeight)
        {
            // 计算相对坐标
            float deltaX = playerPos.X - enemyPos.X;
            float deltaY = playerPos.Y - enemyPos.Y;
            float deltaZ = playerPos.Z - enemyPos.Z;

            // 计算距离
            float distance2D = (float)Math.Sqrt(deltaX * deltaX + deltaY * deltaY);
            float distance3D = (float)Math.Sqrt(deltaX * deltaX + deltaY * deltaY + deltaZ * deltaZ);

            // 计算夹角
            float angleX = 0;
            if (deltaX < 0 && deltaY == 0) angleX = 0;
            else if (deltaX < 0 && deltaY < 0) angleX = (float)(Math.Atan(Math.Abs(deltaY / deltaX)) * 180 / Math.PI);
            else if (deltaX == 0 && deltaY < 0) angleX = 90;
            else if (deltaX > 0 && deltaY < 0) angleX = (float)(90 + Math.Atan(Math.Abs(deltaX / deltaY)) * 180 / Math.PI);
            else if (deltaX > 0 && deltaY == 0) angleX = 180;
            else if (deltaX > 0 && deltaY > 0) angleX = (float)(180 + Math.Atan(Math.Abs(deltaY / deltaX)) * 180 / Math.PI);
            else if (deltaX == 0 && deltaY > 0) angleX = 270;
            else if (deltaX < 0 && deltaY > 0) angleX = (float)(270 + Math.Atan(Math.Abs(deltaX / deltaY)) * 180 / Math.PI);

            float horizontalAngle = mouseAngleX - angleX;
            if (angleX - mouseAngleX > 180) horizontalAngle = 360 - angleX + mouseAngleX;
            if (mouseAngleX - angleX > 180) horizontalAngle = (360 + angleX - mouseAngleX) * -1;

            // 转换坐标
            float opposite = (float)(Math.Sin(horizontalAngle * Math.PI / 180) * distance2D);
            float adjacent = (float)(Math.Cos(horizontalAngle * Math.PI / 180) * distance2D);

            float ratio = distance3D / 500;
            float screenX = (screenWidth / 2) - 0 / ratio + opposite / adjacent * (screenWidth / 2);
            float oppositeY = (float)(Math.Tan(mouseAngleY * Math.PI / 180) * distance2D + deltaZ +40 );
            float screenY = (screenHeight / 2) + oppositeY / distance2D * (screenHeight / 2);

            return new Vector2(screenX, screenY);
        }
    }
}