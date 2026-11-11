# Player animations

Ветка `feature/lowpoly-animations` основана на Foundation v1.

## Риг и источник движения

Используется существующий `character2.fbx`, его валидный Humanoid Avatar и исходные
клипы из `Assets/Game/Animations/Pack`. В импорте сохранён Copy From Other Avatar
с GUID `2d226e59912d0014fbf2646ed5c4b916`. Новые ресурсы из интернета не загружались.

`FunnyMovement` и Rigidbody остаются единственным игровым контроллером.
`PlayerMovement` и CharacterController по-прежнему выключены. Силы движения,
максимальная скорость, прыжок, гравитация и коллайдеры не изменены.
`PlayerAnimationDriver` только читает фактическую `Rigidbody.linearVelocity`:
он не перемещает корень, не прикладывает силы, не меняет скорость тела.
Root Motion выключен; Animator работает в Normal/Always Animate.

## Контроллер

Переработан **существующий** `PlayerMoveAnimation.controller`, его GUID сохранён.

| Состояние | Поведение |
| --- | --- |
| Locomotion | 2D Freeform Cartesian Blend Tree: Idle, Walk, Run, Backward Walk, левый/правый Strafe на двух скоростях |
| Jump | Фаза 0.45–0.95 с существующего jump.fbx, сжата до 0.38 с; начинается при реальном подъёме в воздухе |
| Fall | Зафиксированная совместимая поза из jump.fbx; нисходящее движение или сход с края |
| Landing | Фаза 1.35 с–конец исходного jump.fbx, 0.24 с; затем плавный возврат в Locomotion |
| Interaction | Процедурная накладка рук через Humanoid IK поверх текущего состояния; следует реальному ObjectGrabber.IsHolding |

`Backward Walk.anim` — обратная выборка существующего walking.fbx, а не случайный
клип с другим скелетом. Четыре производных `.anim` содержат Humanoid-кривые;
постоянные кривые хранят по два ключа. Оригинальные FBX не менялись.
Движущиеся клипы и Idle зациклены, root rotation/height/XZ запечены в позу.

Параметры: `MoveX`, `MoveZ`, `Speed`, `VerticalSpeed`, `PlaybackRate`,
`TurnRate` (float); `Grounded`, `Holding` (bool).
MoveX/MoveZ — локальная горизонтальная скорость, сглаженная за 0.08 с.
Speed хранит полную фактическую горизонтальную скорость. Переходы в воздух
реагируют на Grounded и VerticalSpeed без ожидания конца клипа, смешивание 0.08–0.1 с.
Проверка земли идёт от нижней части реального CapsuleCollider, игнорируя собственное
тело и триггеры. Она точнее длинного луча исходного контроллера движения, который
для сохранения механики не изменён.

Walk/Run соответствуют приблизительно 0.16/0.4 мировых единиц в секунду при
исходном масштабе игрока 0.1. Частота подстраивается под фактическую скорость,
PlaybackRate ограничен 2.5. Специального спринта в исходной механике нет: Run
выбирается по достигнутой скорости. Поворот корня остаётся у FirstPersonCamera /
Rigidbody. Клипы turn с запечённым разворотом не включены в дерево: это добавляло
бы второй поворот поверх физического корня.

IK слегка корректирует постановку стоп к поверхности и плавно поднимает руки при
удержании. Он не перемещает предмет и не меняет силу захвата. Штатная дальность
захвата 1 м больше досягаемости рук игрушечного персонажа, поэтому это визуальная
реакция на силовой захват, а не контактные кисти на поверхности предмета.

## Первый человек и предпросмотр

`FirstPersonBodyVisibility` скрывает рендереры тела только на время рисования
FPS-камерой и восстанавливает их после кадра. Тело остаётся видно в Scene View и
других камерах. Основная игровая камера не заменена камерой от третьего лица.

В Editor доступны команды **Tools → Major Damage**:

- **Open Animation Preview**: отдельная `Assets/Game/Scenes/AnimationPreview.unity`.
  Включить Play. Автоцикл показывает Idle → Walk → Run → Back → Left → Right →
  Jump/Landing → Drop/Fall/Landing. Клавиши **1–8** выбирают соответствующий режим,
  **P** возвращает/останавливает автоцикл. Для повторного прыжка переключиться
  с 7 на другой режим и обратно или вызвать Jump preview в контекстном меню компонента.
- **Follow Player in Scene View**: следить за настоящим игровым персонажем снаружи,
  сохраняя FPS-камеру в Game View. Повторная команда отключает слежение.
- **Frame Level** и **Open Main Scene**: обзор уровня и возврат в основную сцену.

Предпросмотр использует тот же Avatar/Controller, увеличенный персонаж x5 и
отдельный физический демонстратор `AnimationPreviewDriver`. Он задаёт скорости
Rigidbody; Animator по-прежнему только читает их. Игровой FunnyMovement в этой
отдельной демонстрации выключен. Демонстратор не добавлен в SampleScene.

## Ограничения

Проверено в Play Mode Unity 6000.6.8f1 на Bazzite. Исходные ProjectVersion и пакеты
6000.0.43f1 сохранены; запуск на точной исходной версии и standalone-сборка
не выполнялись. Отдельный спринт, полноценная контактная система кистей и
абсолютная фиксация стоп на любой скорости не добавлялись. На скоростях,
требующих PlaybackRate > 2.5, возможное скольжение предпочтено неестественной
частоте шагов; параметры физического контроллера при этом не подменяются.

Пересборка через Pipeline: выполнить `BuildPlayerAnimations.Apply` из
`AgentScripts/BuildPlayerAnimations.cs` в открытой SampleScene в Edit Mode,
затем `BuildPlayerAnimations.PreviewScene`. Это пересоздаёт внутренние подресурсы
контроллера, сохраняя GUID самого контроллера и исходных FBX.

Основано на официальной документации Unity:
[2D Blend Trees](https://docs.unity3d.com/6000.0/Documentation/Manual/BlendTree-2DBlending.html),
[Animator Controller API](https://docs.unity3d.com/6000.0/Documentation/ScriptReference/Animations.AnimatorController.html),
[AnimationClipSettings в UnityCsReference](https://github.com/Unity-Technologies/UnityCsReference/blob/master/Editor/Mono/Animation/AnimationClipSettings.bindings.cs).
