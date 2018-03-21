# VrGizmos
Unity's Gizmos functionality doesn't work in VR, so VrGizmos offers a similar immediate-mode way to draw debug visualisation.

It is *not* intended to fully mimic the Gizmos API or reproduce all of its functionality. At the moment functionality is also fairly limited.

## Example
```C#
Sigtrap.VrGizmos.alpha = 0.2f;
Sigtrap.VrGizmos.DrawSphere(transform.position, 1f, Color.green);
```
