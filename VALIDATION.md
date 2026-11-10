# Проверка Foundation v1

Дата: 11 ноября 2026. Ветка: `refactor/foundation-v1`.

## Среда и границы результата

В исходном репозитории сохранены **Unity 6000.0.43f1**, URP 17.0.4 и все версии
`Packages/manifest.json` / `packages-lock.json`. Другой Editor не устанавливался.

Пользователь выбрал проверку без установки исходной версии. Реальные компиляция,
Test Runner и Play Mode выполнены в отдельной копии на установленном **6000.6.8f1**.
Для неё применены имеющиеся локальные пакеты: URP 17.6.0, uGUI 2.6.0,
Input System 1.20.1, Test Framework 1.8.0 и Pipeline 0.10.0-exp.1.
Это проверяет игровой код и сцену в реальном Unity, но не доказывает совместимость
всего исходного набора пакетов, включая Muse, с точной версией 6000.0.43f1.
Пересериализованные новой версией сцены/настройки не копировались обратно в ветку.

Локальная проверочная копия: `/var/home/deck/Desktop/tests/foundation-work/validation`.
Подробные ответы CLI: соседний каталог `foundation-work/reports`.
Проверка выполнена официальным Unity CLI 1.0.0-beta.13 через localhost Pipeline.
CLI сообщил о beta.17; необязательное обновление не выполнялось.

## Выполнено

| Проверка | Результат |
| --- | --- |
| C# игрового кода и тестовых сборок | `compilationFailed=false` |
| EditMode | 10 passed, 0 failed |
| PlayMode | 5 passed, 0 failed |
| Основная сцена | 129 GameObject, 0 missing scripts |
| AudioListener | 1 активный |
| Исходные GUID | 372 сохранены, пропусков и дубликатов нет |
| Основной контроллер | FunnyMovement включён, PlayerMovement выключен |
| Прыжок | один импульс; в сцене 4.2 при массе 1 |
| Захват | 1 м / сила 10; ограничение ускорения и удаление удерживаемого тела проверены |
| Повреждения | реальное падение/столкновение снижает HP и начисляет награду |
| Основная сцена в Play | Time.frameCount действительно увеличивается |
| Реальный ввод Game View | первый щелчок захватывает курсор без захвата предмета; W переместил игрока на 0.542 м; Escape освободил курсор |
| HUD | обновляется вместе с фактическим балансом |
| Разрушение | появляется активный Impact Debris с Rigidbody |
| После исправления дубликата коллайдера | consoleErrors=0, consoleWarnings=0 |

EditMode проверяет ограничение награды при избыточном уроне, мелкие повторные попадания,
некорректный урон, порог/множитель столкновения, буфер и истечение прыжка, баланс/сброс.
PlayMode использует настоящие Rigidbody и физические шаги: одноразовый прыжок,
управление при падении, ограничение захвата, уничтожение удерживаемого тела,
реальное столкновение и одноразовую выплату при уничтожении.

В основной сцене отдельно проверены частичное повреждение, увеличение кошелька,
отображение суммы в HUD и разрушение исходного объекта с созданием обломка.
Сводные ответы и снимок находятся в `Documentation/`.

## Что пришлось исправить при проверке

- Старый модуль `com.unity.modules.vr` не поддерживается Unity 6.6, registry потребовал
  авторизацию, Test Framework 1.4.6 не компилировался в 6.6. Эти вопросы решены только
  в проверочной копии за счёт подходящих локальных пакетов.
- После Safe Mode потребовался чистый перезапуск проверочного Editor.
- Диалоги обновления/сохранения проверочной сцены блокировали Pipeline и вызывали
  тайм-ауты. Они закрыты, результаты тестов получены после фактического завершения.
- Устаревшее свойство TMP заменено на `textWrappingMode`, которое имеется и в исходном uGUI.
- У стандартной машины оказался второй вогнутый MeshCollider рядом с рабочим выпуклым.
  Дубликат отключён; повторный Play Mode не воспроизвёл ошибку. Проверка использует
  `GetComponentInParent<Rigidbody>`, поскольку недопустимый коллайдер может возвращать
  null в `attachedRigidbody`.

Справка по коллайдеру: [Mesh collider component reference, Unity 6.0](https://docs.unity3d.com/6000.0/Documentation/Manual/class-MeshCollider.html).

## Воспроизведение проверок

В точной исходной версии открыть `Assets/Game/Scenes/SampleScene.unity`, запустить
Test Runner и выбрать сборки `Foundation.EditMode.Tests` / `Foundation.PlayMode.Tests`.
Затем выполнить ручной сценарий из README.

Проверка переноса ассетов без Unity:

```sh
python3 Tools/verify_assets.py
```

Для Editor с Pipeline (путь указывать явно):

```sh
unity command script recompile_status --project-path "<project>" --json
unity command test run --mode editor --filter FoundationStateTests --async-tests true --project-path "<project>" --json
unity command test status --project-path "<project>" --json
unity command test run --mode playmode --filter FoundationPhysicsTests --async-tests true --project-path "<project>" --json
unity command test status --project-path "<project>" --json
unity command script run --file AgentScripts/FoundationAudit.cs --entry FoundationAudit.Inspect --project-path "<project>" --json
```

`RuntimeEvidence` предназначен для отдельного свежего Play Mode: он намеренно повреждает
и разрушает тестовые объекты основной сцены. Обычный аудит `Inspect` ничего не меняет.
В исходный проект Pipeline не добавлялся как новая обязательная зависимость.

## Не проверено

- Запуск и Test Runner именно в Unity 6000.0.43f1 с исходными Muse/URP-пакетами.
- Standalone-сборка, другие ОС, геймпад и распределение FPS на разных GPU.
- Длительный игровой баланс, сохранения между запусками и несколько уровней.

Коммиты сделаны с локальной подписью Codex; глобальный Git config не менялся.
Push и merge в main не выполнялись. Исходные ассеты, локальный Library и старая история сохранены.
После проверки Play Mode остановлен, Run In Background восстановлен, auto tick отключён.
Editor оставлен открытым на основной сцене проверочной копии.
