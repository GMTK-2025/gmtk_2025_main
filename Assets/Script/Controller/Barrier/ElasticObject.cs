using System;
using UnityEngine;

public class ElasticObject : MonoBehaviour
{
    public float AnimPlayTime = 0.7f;
    [SerializeField] private Vector2 _elasticity = new Vector2(0f, 10f);
    private static readonly int HitPosition = Shader.PropertyToID("_HitPosition");
    private static readonly int AnimProgress = Shader.PropertyToID("_AnimProgress");
    private bool _isPlayAnim = false;
    private Vector2 _collisionPoint = Vector2.zero;
    private float _animPlayingTime = 0f;
    private BoxCollider2D _collider;
    private MaterialPropertyBlock _block;
    private SpriteRenderer _renderer;

    private void Awake()
    {
        _collider = GetComponent<BoxCollider2D>();
        _renderer = GetComponent<SpriteRenderer>();
        _block = new MaterialPropertyBlock();
        _renderer.GetPropertyBlock(_block);
    }

    void Update()
    {
        if (_isPlayAnim)
        {
            _block.SetVector(HitPosition, new Vector4(_collisionPoint.x, _collisionPoint.y, 0, 0));
            _block.SetFloat(AnimProgress, _animPlayingTime / AnimPlayTime);
            _renderer.SetPropertyBlock(_block);
            _animPlayingTime -= Time.deltaTime;
            if (_animPlayingTime <= 0f)
            {
                _block.SetFloat(AnimProgress, 0f);
                _renderer.SetPropertyBlock(_block);
                _isPlayAnim = false;
            }
        }
    }

    private void OnCollisionEnter2D(Collision2D other)
    {
        Rigidbody2D rb = other.gameObject.GetComponent<Rigidbody2D>();
        rb.AddForce(_elasticity, ForceMode2D.Impulse);

        _collisionPoint = other.GetContact(0).point;
        _isPlayAnim = true;
        _animPlayingTime = AnimPlayTime;
    }
}