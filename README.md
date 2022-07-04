
# Moon Behavior Tree

事件驱动型行为树，类似于UE4的行为树。

indev...

<img src="https://github.com/Sarofc/com.saro.moonbt/blob/dev/doc/editor.jpg" width="80%">

## 一、特性

1. 事件驱动，拥有中断机制。不需要每次都从根节点进行遍历。
2. 基于GraphView的可视化工具，可编辑，可预览运行时状态。

## 二、节点

1. 主要节点种类

    | Node Type | Info                                                                    |
    | --------- | ----------------------------------------------------------------------- |
    | Composite | 定义一个分支的执行规则。                                                |
    | Decorator | 附着于另一个节点，决定树中的一个分支（节点）是否执行。                  |
    | Service   | 附着于Composite节点上，只要其分支正在执行，它们就会以所定义的频率执行。 |
    | Task      | 具体的执行操作，唯一的叶节点。                                          |

2. 详细节点信息

    - Composite Node

        | Composite Node     | Info                                                                                                                                                                                     |
        | ------------------ | ---------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------- |
        | **Selector**       | 节点按从左到右的顺序执行其子节点。当其中一个子节点执行成功时，选择器节点将停止执行。如果选择器的一个子节点成功运行，则选择器运行成功。如果选择器的所有子节点运行失败，则选择器运行失败。 |
        | **Sequence**       | 节点按从左到右的顺序执行其子节点。当其中一个子节点失败时，序列节点也将停止执行。如果有子节点失败，那么序列就会失败。如果该序列的所有子节点运行都成功执行，则序列节点成功。               |
        | **SimpleParallel** | 节点允许一个主任务节点沿整个的行为树执行。主任务完成后，结束模式（Finish Mode） 中的设置会指示该节点是应该立即结束，同时中止次要树，还是应该推迟结束，直到次要树完成。返回主任务的状态。 |

    - Composite Node

        | Decorator Node       | Info                                                                                                     |
        | -------------------- | -------------------------------------------------------------------------------------------------------- |
        | **BBCondition**      | 节点将判断黑板键的值与给定值之间的关系，根据结果（可以是大于、小于、等于，等等）阻止或允许节点的执行。   |
        | **CompareBBEntries** | 节点将比较两个黑板键的值，并根据结果（等于或不等）阻止或允许节点的执行。                                 |
        | **Cooldown**         | 将锁定节点或分支的执行，直到冷却时间结束。                                                               |
        | **ForceSuccess**     | 装饰器会将节点结果更改为成功。                                                                           |
        | **Loop**             | 装饰器会对节点或分支进行多次循环或无限次循环。                                                           |
        | **TimeLimit**        | 装饰器会对节点或分支进行运行时间限制，当时间超过限制，节点或分支仍在运行，则终止其运行，装饰器返回失败。 |

    - Service Node

        | Service Node | Info |
        | ------------ | ---- |
        | **暂无**     |      |

    - Task Node

        | Task Node    | Info                                         |
        | ------------ | -------------------------------------------- |
        | **Wait**     | 使树在此节点上等待，直至指定的等待时间结束。 |
        | **PrintLog** | 打印字符串，调试用                           |

3. 定义自己的节点

    > 继承BTComposite，自定义复合节点</br>
    > 继承BTDecorator，自定义装饰节点</br>
    > 继承BTService，自定义服务节点</br>
    > 继承BTTask，自定义任务节点</br>

    注：⚠ 自定义Composite、Decorator节点，相对复杂，因为要考虑到AbortType。</br>

## 三、中断机制

条件满足时，会按照以下表格进行中断处理。

| Aborts Type        | Info                                         |
| ------------------ | -------------------------------------------- |
| **NONE**           | 不做任何处理                                 |
| **SELF**           | 中断自己                                     |
| **LOWER_PRIORITY** | 中断右边(优先级低，即同一分支序号越大)的节点 |
| **BOTH**           | SELF & LOWER_PRIORITY                        |

Decorator节点位于Composite节点以下时(即Decorator节点，向跟节点回溯，遇到的第一个Composite节点)，AbortType属性可以生效, 且每种Composite类型的支持程度不同。

| Composite Type     | Info                                                     |
| ------------------ | -------------------------------------------------------- |
| **Selector**       | 全部AbortsType                                           |
| **Sequence**       | NONE & SELF                                              |
| **SimpleParallel** | 只有NONE有效(TODO:还需要考虑此节点下的其他Composite节点) |

## 四、黑板

黑板可以称为AI的“大脑”，但它并没有实际逻辑，仅仅是存储数据，在合适时机，抛出对应的事件。</br>
只能编辑黑板条目，不支持编辑时赋值。运行时，可通过service/task节点给黑板条目赋值。

## TODO

- [ ] 黑板
- [ ] 更多节点
- [ ] 编辑器
- [ ] 单元测试
- [ ] json支持

## 参考链接

BonsaiBehaviourTree <https://github.com/luis-l/BonsaiBehaviourTree>

NPBehave <https://github.com/meniku/NPBehave>

UE4 Doc <https://docs.unrealengine.com/zh-CN/Engine/ArtificialIntelligence/BehaviorTrees/index.html>