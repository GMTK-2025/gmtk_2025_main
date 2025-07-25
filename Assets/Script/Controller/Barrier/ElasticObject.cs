using System;
using UnityEngine;

public class ElasticObject : MonoBehaviour
{
    [SerializeField] private Vector2 _elasticity = new Vector2(0f, 10f);
    private static readonly int CollisionWs = Shader.PropertyToID("CollisionWS");
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

    void Start()
    {
    }

    void Update()
    {
    }

    private void OnCollisionEnter2D(Collision2D other)
    {
        Rigidbody2D rb = other.gameObject.GetComponent<Rigidbody2D>();
        Vector2 collision = other.GetContact(0).point;
        rb.AddForce(_elasticity, ForceMode2D.Impulse);
        _block.SetVector(CollisionWs, new Vector3(collision.x, collision.y, 0f));
        _renderer.SetPropertyBlock(_block);
    }
}