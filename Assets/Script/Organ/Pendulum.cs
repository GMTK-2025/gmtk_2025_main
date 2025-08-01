using UnityEngine;

namespace Script.Organ
{
    public class Pendulum : MonoBehaviour
    {
        [Header("摆动设置")]
        [Range(10, 180)] public float swingAngle = 60f; 
        public float swingSpeed = 1f;
        public bool startRight = true;

        private bool swingingRight;
        private float currentAngle;

        private void Start()
        {
            swingingRight = startRight;
            currentAngle = startRight ? swingAngle : -swingAngle;
        }

        private void Update()
        {
            // 计算摆动
            float targetAngle = swingingRight ? swingAngle : -swingAngle;
            currentAngle = Mathf.MoveTowardsAngle(currentAngle, targetAngle, swingSpeed * 100 * Time.deltaTime);

            // 到达极限时反转方向
            if (Mathf.Abs(currentAngle - targetAngle) < 0.1f)
            {
                swingingRight = !swingingRight;
            }

            // 直接旋转父物体
            transform.localRotation = Quaternion.Euler(0, 0, currentAngle);
        }
    }
}