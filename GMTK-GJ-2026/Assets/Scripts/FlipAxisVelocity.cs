using CMF;
using UnityEngine;

public class FlipAxisVelocity : MonoBehaviour
{
    [SerializeField]
    SpriteRenderer _sprite;
    [SerializeField]
    SimpleWalkerController _controller;
    [SerializeField]
    bool flip;

    private void Update()
    {
        if (_controller.GetMovementVelocity().x < 0)
        {
            _sprite.flipX = flip;
        } else
        {
            _sprite.flipX = !flip;
        }
    }
}
