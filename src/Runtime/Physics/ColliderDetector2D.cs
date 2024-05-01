using UnityEngine;
using UnityEngine.Events;

namespace Yu5h1Lib.Game.Character
{
    public class ColliderDetector2D : Rigidbody2DBehaviour
    {
        public new Collider2D collider;

        #region Ground detection parameters
        private ContactFilter2D GroundFilter;
        public LayerMask GroundLayer => GroundFilter.layerMask;
        private RaycastHit2D[] GroundCastResults;
        public RaycastHit2D groundHit { get; private set; }
        public bool IsGrounded { get; private set; }
        [SerializeField,Range(0.00001f,1.0f)]
        private float groundRayDistance = 0.2f;
        [SerializeField, Range(0.00001f, 1.0f)]
        private float groundRayOffset = 0.25f;
        [SerializeField, Range(0.01f, 1.0f)]
        private float groundDistanceThreshold = 0.05f;
        #endregion

        public UnityEvent<bool> OnGroundStateChangedEvent;
        public Vector2 offset => collider.offset; 
        public Vector2 extents { get; private set; }
        public Vector2 up => (Vector2)transform.up;
        public Vector2 down => -up;
        public Vector2 right => transform.right;
        public Vector2 center => rigidbody.position + (transform.right * new Vector2(offset.x,0)) + (transform.up * new Vector2(0,offset.y));
        public Vector2 front => center + (right * extents.x);
        public Vector2 top => center + (up * extents.y);
        public Vector2 bottom => center + (down * extents.y);
        private int PlatformLayer;
        protected override void Reset()
        {
            if (TryGetComponent(out Controller2D character))
                collider = GetComponent<CapsuleCollider2D>();
        }
        void Awake()
        {
            GroundFilter = new ContactFilter2D()
            {
                useLayerMask = true,
                layerMask = LayerMask.GetMask("Platform", "PhysicsObject")
            };
            if (collider == null && TryGetComponent(out CapsuleCollider2D c))
                collider = c;
            PlatformLayer = LayerMask.NameToLayer("Platform");
            Init();
        }
        public void Init()
        {
            extents = collider.GetSize() * 0.5f;
        }
        private void OnDrawGizmosSelected()
        {
            if (groundHit)
            {
                var color = Gizmos.color;
                var reverseHroundHitPoint = groundHit.point + groundHit.normal ;
                Gizmos.DrawSphere(reverseHroundHitPoint, 0.1f);

                Gizmos.color = Color.magenta;
                Gizmos.DrawWireSphere(groundHit.collider.ClosestPoint(reverseHroundHitPoint), 0.05f);
                Gizmos.color = Color.yellow;
                Gizmos.DrawWireSphere(groundHit.point, 0.15f);
                Gizmos.color = color;
            }    
        }
        private void LateUpdate()
        {
            if (IsGrounded && groundHit.collider.gameObject.layer == PlatformLayer)
                CheckPlatformStandHeight();
        }
        //private void OnCollisionEnter2D(Collision2D collision)
        //{
            
        //}
        /// <summary>
        /// returb slopDir
        /// </summary>
        /// <param name="right"></param>
        public Vector2 CheckSlop2D(bool right)
        {
            Vector2 refRir = right ? transform.up : -transform.up;
            #region Deprecated
            //var groundNormal = transform.InverseTransformDirection(groundHit.normal);
            //float angle = Vector2.Angle(refRir, groundNormal);
            //float s = angle * Mathf.Deg2Rad;
            //var slopDir = new Vector2(Mathf.Cos(s), groundNormal.x > 0 ? -Mathf.Sin(s) : Mathf.Sin(s)).normalized; 
            #endregion
            var n = groundHit.normal;
            /// simple solution
            var slopDir = right ? new Vector2(n.y, -n.x) : new Vector2(-n.y, n.x);
#if UNITY_EDITOR
            Debug.DrawRay(groundHit.point, slopDir.normalized * 2, Color.green);
#endif
            return slopDir;
        }

        public void CheckGround()
        {
            GroundCastResults = new RaycastHit2D[5];
            groundHit = default(RaycastHit2D);
            collider.Cast(-transform.up, GroundFilter, GroundCastResults, groundRayDistance, true);
            foreach (var hit in GroundCastResults)
            {
                if (!hit)
                    continue;
                var p = (Vector2)transform.InverseTransformPoint(hit.point);
                var normal = transform.InverseTransformDirection(hit.normal);
                var localbottom = (Vector2)transform.InverseTransformPoint(bottom);
                /// normal.y > 0.5f = slop angle > 45�X
                if (normal.y > 0.5f && p.y <= localbottom.y + groundRayOffset && hit.distance < groundDistanceThreshold)
                    groundHit = hit;
#if UNITY_EDITOR
                    Debug.DrawLine(center, hit.point, Color.green);
                    Debug.DrawLine(center, hit.point + hit.normal * hit.distance, Color.magenta);
#endif
            }
#if UNITY_EDITOR
            if (groundHit)
                Debug.DrawLine(center, groundHit.point, Color.yellow);
            else
                Debug.DrawLine(center, bottom );
#endif
            if (IsGrounded == groundHit)
                return;
            IsGrounded = groundHit;
            OnGroundStateChanged();
            if (IsGrounded)
            {                
                CheckPlatformStandHeight(0);
            }
                
        }
        void OnGroundStateChanged()
        {
            OnGroundStateChangedEvent?.Invoke(IsGrounded);
        }
        public void CheckPlatformStandHeight(float Threshold = 0.05f)
        {            
            if (!groundHit)
                return;
            var surfaceHitPoint = groundHit.collider.ClosestPoint(groundHit.point + groundHit.normal);
            var localclosetGroundPoint = (Vector2)transform.InverseTransformPoint(surfaceHitPoint);
            var localBottomY = -extents.y;
            var distance = localBottomY.Distance(localclosetGroundPoint.y);
            if (Threshold <= 0 || distance > Threshold)
            {
                transform.position = transform.TransformPoint(0,Mathf.
                    Sign(localclosetGroundPoint.y - localBottomY ) * distance);
                Debug.Log("Fix incorrect height caused by rigibody momentum");
            }
        }
        public bool CheckEdge()
        {
            if (!collider || !IsGrounded)
                return false;
            var pos = bottom + (right * extents.x);
            var hitground = Physics2D.Raycast(pos,down, 1, GroundLayer.value);
            pos += up * 0.1f;
            if (hitground)
            {
                Debug.DrawRay(pos, down * (hitground.distance + 0.1f), Color.white, Time.deltaTime);
                return false;
            }
            Debug.DrawRay(pos, down, Color.white, Time.deltaTime);
            return true;
        }
    }
}