# Scripts de Botellous: parte del nivel 3

Esta guía describe los scripts de juego de esta carpeta. Está pensada para
entender y configurar la parte del proyecto, no para describir los niveles o
recursos creados por el resto del equipo.

## Qué hace cada script

- `PlayerMov.cs`: lee el teclado y el ratón, mueve y hace saltar al jugador,
  actualiza su Animator y aplica el daño del ataque.
- `PlayerDamageReceiver.cs`: guarda la vida del jugador. Al llegar a cero,
  muestra un mensaje en la consola y desactiva el objeto del jugador.
- `BossComportamiento.cs`: persigue al jugador, decide cuándo atacar, recibe
  daño y cambia de fase cuando le queda la mitad de la vida.
- `GameManager.cs`: coordina la apertura de la puerta cuando el jugador entra
  en el trigger correspondiente.

Los valores que se ajustan desde Unity están serializados y se editan en el
Inspector. La vida actual se reinicia a la vida máxima al comenzar la partida.

## Controles y mecánicas

- **A / D:** moverse a izquierda y derecha.
- **W:** saltar cuando el personaje está en el suelo.
- **Botón izquierdo del ratón:** atacar; hay un breve tiempo de espera entre
  ataques.
- El jefe se acerca al jugador y usa un golpe cuando está cerca. A más distancia
  puede usar gas; al perder la mitad de su vida aumenta la posibilidad de ese
  ataque. Su movimiento está limitado a X=44–66.
- El filtro de humo (`playerHasSmokeFilter`) evita el daño del gas. La zona
  visual (`toxicGasZone`) es opcional; si no se asigna, el ataque todavía puede
  hacer daño. Al tocar un collider del objeto `mascaraGas` (o de un hijo suyo),
  el jugador activa el filtro del jefe; el collider puede ser trigger o sólido.
- `ApplyValveStun` permite que otro componente aturda al jefe durante un tiempo.

## Configuración en Unity

### Jugador

El objeto del jugador necesita `Rigidbody2D`, `Animator` y `BoxCollider2D`.
También necesita `PlayerDamageReceiver`; el objeto raíz debe tener la etiqueta
`Player`. Asigna en el Animator los parámetros `IsMoving`, `IsGrounded` y
`Attack`. El evento de impacto del clip de ataque debe llamar a
`PlayerMov.OnAnimationAttackHit`.

En `PlayerMov`, configura `Ground Layers` para que el raycast de suelo detecte
las plataformas y no al propio jugador.

### Puerta y GameManager

El objeto `TriggerPuerta` lleva `GameManager` y un `BoxCollider2D` con la opción
**Is Trigger** activada. En el Inspector, asigna el Tilemap `Puerta` a
`Door Tilemap` y el objeto `Grid` al campo `Grid`. Al entrar el jugador
(etiqueta `Player`) en el trigger, el Tilemap baja cuatro celdas de la
cuadrícula una sola vez. El tamaño del movimiento se basa en las celdas del
`Grid`, no en una distancia fija.
El campo `Boss` debe apuntar al componente `BossComportamiento`. Hasta que el
jugador entre en el trigger, el jefe y su simulación física permanecen
desactivados; al abrirse la puerta, ambos se activan.

`GameManager` centraliza la lógica general de esta escena. Los scripts de
movimiento, vida del jugador y comportamiento del jefe permanecen separados
porque pertenecen a componentes específicos de esos objetos.

### Jefe

El objeto del jefe necesita `Rigidbody2D`, `Animator` y `BoxCollider2D`.
Asigna el jugador y, si existe, la zona visual del gas en el Inspector. El
Animator debe tener los parámetros `Trigger_Slam`, `Trigger_Gas`, `Trigger_Hit`,
`Die`, `IsStunned` e `IsPhase2`.

Los clips de animación del jefe deben llamar a estos eventos:

- Animación de golpe: `BossComportamiento.OnAnimationTriggerSlam`.
- Animación de gas: `BossComportamiento.OnAnimationEmitGas`.

Los nombres de parámetros y eventos distinguen mayúsculas y minúsculas. Si uno
no coincide, Unity no podrá ejecutar la transición o el evento correspondiente.

## Alcance y límites conocidos

Esta carpeta implementa movimiento, salto, ataque, vida y combate contra el
jefe. No implementa menú, HUD, pantallas de victoria/derrota ni sistema de
sonido. Esos elementos pertenecen a otras partes del proyecto y no se han
cambiado aquí. Por tanto, esta entrega documenta la parte de código del nivel 3,
pero por sí sola no acredita los criterios de UI o sonido de la rúbrica.
