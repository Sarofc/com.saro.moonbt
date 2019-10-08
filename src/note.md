# NOTE

| Node Type                  | parent Node Type | AbortType            |
| -------------------------- | ---------------- | -------------------- |
| Cooldown                   | Selector         | NONE、LOWER_PRIORITY |
|                            | Sequence         | NONE                 |
| TimeLimit                  | Any              | SELF                 |
| Blackboard                 | Selector         | Any                  |
|                            | Sequence         | NONE、SELF           |
| Compare Blackboard entries | Selector         | Any                  |
|                            | Sequence         | NONE、SELF           |
| Loop                       | x                | x                    |

note : SimpleParallel 全部为 NONE
