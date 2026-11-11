# Visual & Animation Update — проверка

11 ноября 2026. Ветка `feature/lowpoly-animations`, база — `refactor/foundation-v1`.

## Условия

Реальный Unity Editor **6000.6.8f1** на Bazzite, официальный Unity CLI
**1.0.0-beta.17**, локальный Pipeline **0.10.0-exp.1**. CLI обновлён с beta.13
согласно инструкции установленного Unity-плагина. Другой Editor не устанавливался.
Проверочная копия: `/var/home/deck/Desktop/tests/foundation-work/validation`.
Исходный ProjectVersion **6000.0.43f1**, manifest и packages-lock сохранены.
Условия локальных совместимых пакетов проверочной копии перечислены в
[предыдущем отчёте](../VALIDATION.md).

Материалы, Animator и обе сцены построены через API живого Editor, затем перенесены
в ветку. Это не обновляет ProjectVersion, но новые сериализованные ресурсы
созданы более новым Editor. Совместимость полного исходного набора пакетов и
ресурсов именно с 6000.0.43f1 здесь не доказана.

## Результат

| Проверка | Фактический результат |
| --- | --- |
| Компиляция | completed, failed=false, compilationFailed=false |
| EditMode | **13 passed**, 0 failed, 0 skipped |
| PlayMode | **8 passed**, 0 failed, 0 skipped |
| Основная сцена | 137 GameObject; 0 missing scripts; 1 активный AudioListener |
| Физические компоненты | FunnyMovement включён, альтернативный PlayerMovement выключен; недопустимых динамических MeshCollider нет |
| Настройки | mass=1, moveForce=8, jumpForce=4.2, maxSpeed=5; grabDistance=1, grabForce=10 сохранены |
| Разрушаемые объекты | 4 исходных объекта с прежними HP, порогами, ценой и ссылками на обломки |
| Исходные ресурсы | все 372 исходных GUID сохранены, дубликатов нет; ThirdParty и Packages/ProjectSettings не изменены |
| Кадры | серверный job wait с Time.frameCount changed → met=true; playerLoopTicking=true |
| Console | consoleErrors=0, consoleWarnings=0, compilationFailed=false после проверки |

10 исходных EditMode-тестов Foundation дополнены проверками восьми движений Blend
Tree / воздушных состояний, Humanoid-импорта с исходным Avatar и матовых материалов.
Пять исходных PlayMode-тестов Foundation дополнены настоящим ригом и Rigidbody:
скорость выбирает forward/back/strafe, кости действительно меняют позу, остановка
возвращает Idle, физический импульс проходит Jump/Fall/Landing, падение без импульса
переходит сразу в Fall. Animator не добавляет скорости телу.

## Интеграционные действия в основной сцене

- Настоящие WASD/Space через сфокусированный Game View, без подмены Animator-параметров.
  Передний/задний ход и оба боковых движения дали ожидаемый знак MoveX/MoveZ.
  Прыжок: Jump при вертикальной скорости **+2.434**, Fall при **−2.078**,
  после касания — Grounded=true и Locomotion. Root Motion всё время false.
- FPS-вид прочитан и осмотрен; тело/голова не перекрывают обзор. Внешняя камера
  одновременно показывает полный анимированный персонаж.
- Исходный **Scrap Bin B** захвачен настоящим `ObjectGrabber.TryGrab` через его
  raycast в пределах штатного метра. Для воспроизводимости игрок размещён рядом
  с контейнером только в Play Mode; камера на время теста зафиксирована.
  Удержание сохранилось после физических шагов: Holding=true, вес реакции рук=1,
  масса предмета=1 и useGravity=true. Кисти поднялись в рабочую позу. После
  освобождения курсора удержание исчезло. Все временные изменения отброшены при Stop.
- Прежний `FoundationAudit.RuntimeEvidence` реально снял 10 HP, начислил 10 единиц,
  разрушил стандартную машину, создал активный Impact Debris с Rigidbody и
  обновил HUD до **$10,010.00**. Сумма учитывает повреждения от исходного падения
  машины в начале сцены, поэтому выплата при добивании ограничена остатком стоимости.

Первый короткий тест ввода не захватил фокус Game View — это выявлено чтением
Application.isFocused/Cursor. Он не засчитан. Результаты выше получены при
focused=true, cursor=Locked. Один GUI-щелчок завершил нашу тестовую Play-сессию;
она была запущена заново. Ошибок игрового кода или Animator при этом не обнаружено.
В журнале Pipeline сохранилось стартовое предупреждение Unity о будущем
устаревании legacy Input Manager; переход на другую систему ввода не входил в задачу.

При последнем выходе из Play Mode нативная Unity Console показала две служебные
ошибки **Pipeline**: `ObjectDisposedException` для `HttpListenerResponse` при ответе
на `/api/exec` во время перезагрузки домена. Журнал `console get` их не отобразил;
они прочитаны дополнительно через нативные `LogEntries` и сохранены в
`verification.json`. Это ограничение текущего Pipeline 0.10, а не ошибка
PlayerAnimationDriver/Animator. После диагностики Console очищена, повторный аудит
Edit Mode не добавил ошибок. Игровые тесты и предыдущие Play Mode-проверки завершились
без ошибок. Версия Pipeline и исходные пакеты ради этого не менялись.

## Кадры и видео

Каталог [VisualAnimation](VisualAnimation): обзор и уличный ракурс, FPS-вид,
Idle/Walk/Run/Backward/Left/Right, Jump/Fall/Landing, реальное удержание контейнера.
В `verification.json` сохранены ответы тестов, снимки Rigidbody/Animator, выбранные
клипы с весами и проверка игровых систем.

[AnimationPreview.mp4](VisualAnimation/AnimationPreview.mp4) — около 13.5 с,
960×600, H.264 / 24 кадра/с. Кадры получены **с камеры живой Play-сцены**, пока
реальные Rigidbody и Animator продолжали обновляться; это не офлайн-проигрывание
клипов. PNG записываются реже 24 Гц, FFmpeg использует интервалы игрового времени
и формирует поток 24 Гц с повторением кадров. `video-timing.json` содержит
исходные игровые отметки времени и Time.frameCount. Запись X11 экрана на Wayland
оказалась чёрной и была заменена прямой записью камеры; чёрные кадры не включены.

После проверки Editor возвращён к SampleScene в **Edit Mode**, сцена сохранена,
auto tick выключен, session override Run In Background снят.

## Повторить

В открытом проекте с Pipeline, всегда передавая его собственный путь:

```sh
unity command test run --mode editor --filter Foundation.EditMode.Tests --filter-type assembly --async-tests true --project-path "<project>" --caller plugin --skill unity-pipeline --json
unity command test status --project-path "<project>" --caller plugin --skill unity-pipeline --json
unity command test run --mode playmode --filter Foundation.PlayMode.Tests --filter-type assembly --async-tests true --project-path "<project>" --caller plugin --skill unity-pipeline --json
unity command test status --project-path "<project>" --caller plugin --skill unity-pipeline --json
unity command script run --file AgentScripts/FoundationAudit.cs --entry FoundationAudit.Inspect --project-path "<project>" --caller plugin --skill unity-pipeline --json
```

Для ручного полного тела — Tools → Major Damage → Open Animation Preview.
Для обычной игры — Open Main Scene. Перед длительной проверкой агентом включать
`editor set_auto_tick` и session override `editor.playmode set_run_in_background`,
а после проверки возвращать их обратно. Сам исходный проект не получил новой
обязательной зависимости Pipeline.

## Не выполнено / границы

- Не запускался точный Editor 6000.0.43f1, не собирался standalone-плеер.
- Не проверялись другие ОС, геймпад, широкий диапазон FPS/GPU и длительный баланс.
- Нет отдельной механики спринта, контактной системы пальцев для любых предметов
  или абсолютной фиксации стоп на любой скорости. При исходном игрушечном масштабе
  0.1 и пределе 5 единиц/с ограничение частоты анимации может оставлять скольжение
  на высоких скоростях. Физические параметры ради маскировки этого не менялись.
- Merge в main и переписывание истории не выполнялись.
