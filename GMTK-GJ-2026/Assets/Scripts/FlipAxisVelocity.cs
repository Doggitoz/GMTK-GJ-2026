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

    Vector3 _lastPosition;

    private void Update()
    {
        if (_controller)
        {
            if (_controller.GetMovementVelocity().x < 0)
            {
                _sprite.flipX = flip;
            }
            else if (_controller.GetMovementVelocity().x > 0)
            {
                _sprite.flipX = !flip;
            }
        } else
        {
            Vector3 currentPosition = _sprite.transform.position;
            Vector3 velocity = (currentPosition - _lastPosition) / Time.deltaTime;
            if (velocity.x <= 0)
            {
                _sprite.flipX = flip;
            }
            else
            {
                _sprite.flipX = !flip;
            }

            _lastPosition = _sprite.transform.position;
        }
    }
}
