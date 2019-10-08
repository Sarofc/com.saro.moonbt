# BehaviorTree

事件驱动型行为树，类似于UE4的行为树。

>> ## 特性

1. 事件驱动，拥有中断机制。不需要每次都从根节点进行遍历。
2. 仅包含runtime，完全使用代码构建行为树，提供一个可视化debug工具。

>> ## 节点

1. 主要节点种类

    | Node Type | Info                                                                    |
    | --------- | ----------------------------------------------------------------------- |
    | Composite | 定义一个分支的执行规则。                                                |
    | Decorator | 附着于另一个节点，决定树中的一个分支（节点）是否执行。                  |
    | Service   | 附着于Composite节点上，只要其分支正在执行，它们就会以所定义的频率执行。 |
    | Task      | 具体的执行操作，唯一的叶节点（Leaf Node）。                             |

2. 详细节点信息

    - Composite Node
        | Composite Node     | Info                                                                                                                                                                                     |
        | ------------------ | ---------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------- |
        | **Selector**       | 节点按从左到右的顺序执行其子节点。当其中一个子节点执行成功时，选择器节点将停止执行。如果选择器的一个子节点成功运行，则选择器运行成功。如果选择器的所有子节点运行失败，则选择器运行失败。 |
        | **Sequence**       | 节点按从左到右的顺序执行其子节点。当其中一个子节点失败时，序列节点也将停止执行。如果有子节点失败，那么序列就会失败。如果该序列的所有子节点运行都成功执行，则序列节点成功。               |
        | **SimpleParallel** | 节点允许一个主任务节点沿整个的行为树执行。主任务完成后，结束模式（Finish Mode） 中的设置会指示该节点是应该立即结束，同时中止次要树，还是应该推迟结束，直到次要树完成。                   |

    - Composite Node
        | Decorator Node          | Info                                                                                                     |
        | ----------------------- | -------------------------------------------------------------------------------------------------------- |
        | **BlackboardCondition** | 节点将判断黑板键的值与给定值之间的关系，根据结果（可以是大于、小于、等于，等等）阻止或允许节点的执行。   |
        | **CompareBBEntries**    | 节点将比较两个黑板键的值，并根据结果（等于或不等）阻止或允许节点的执行。                                 |
        | **Condition**           | 作用同 BlackboardCondition，但不使用黑板                                                                 |
        | **Cooldown**            | 将锁定节点或分支的执行，直到冷却时间结束。                                                               |
        | **ForceSuccess**        | 装饰器会将节点结果更改为成功。                                                                           |
        | **Loop**                | 装饰器会对节点或分支进行多次循环或无限次循环。                                                           |
        | **TimeLimit**           | 装饰器会对节点或分支进行运行时间限制，当时间超过限制，节点或分支仍在运行，则终止其运行，装饰器返回失败。 |

    - Service Node
        | Service Node       | Info         |
        | ------------------ | ------------ |
        | **DefaultService** | 默认服务节点 |

    - Task Node
        | Task Node   | Info                                                                                    |
        | ----------- | --------------------------------------------------------------------------------------- |
        | **Actions** | 默认行动节点                                                                            |
        | **Wait**    | 任务节点可以在行为树中使用，使树在此节点上等待，直至指定的 等待时间（Wait Time） 结束。 |
        | **Log**     | 打印字符串                                                                              |

3. 类图与结构图

    <img src="https://github.com/Sarofc/BehaviorTree-Unity/blob/master/src/cd.jpg?raw=true"></br>

    <img src="https://github.com/Sarofc/BehaviorTree-Unity/blob/master/src/structure.jpg?raw=true">

4. 定义自己的节点

    > 继承Composite，自定义复合节点</br>
    > 继承Decorator，自定义装饰节点</br>
    > 继承Service，自定义服务节点</br>
    > 继承Task，自定义任务节点</br>

    注：⚠ 自定义Composite、Decorator节点，相对复杂，因为要考虑到ObserverAborts。</br>

>> ## 中断机制

条件满足时，会按照以下表格进行中断处理。

| Aborts Type        | Info                     |
| ------------------ | ------------------------ |
| **NONE**           | 不做任何处理             |
| **SELF**           | 中断自己                 |
| **LOWER_PRIORITY** | 中断右边(优先级低)的节点 |
| **BOTH**           | SELF & LOWER_PRIORITY    |

Decorator节点位于Composite节点以下时，ObserverAborts属性可以生效, 且每种Composite类型的支持程度不同。

| Composite Type     | Info           |
| ------------------ | -------------- |
| **Selector**       | 全部AbortsType |
| **Sequence**       | NONE & SELF    |
| **SimpleParallel** | 只有NONE有效   |

>> ## 黑板

黑板可以称为AI的“大脑”，但它并没有实际逻辑，仅仅是存储数据，在合适时机，抛出对应的事件。</br>
由于本行为树为纯代码构建，完全可以使用类来充当数据结构，所以黑板并不是必须的，这样也能避免黑板所带来的性能损失。</br>

```csharp
var bb = UBTContext.Instance.GetGlobalBlackboard("DefaultBB");//获取全局黑板
//var bb = new Blackboard("BB");//实例化黑板
bb.Set("key1", 10);//设置键的时候会自动绑定OnValueChanged事件，当键值发生改变时，会抛出事件（此时为空事件）。
bb.AddObserver("key1",()=>UnityEngine.Debug.Log("callback")));//为key1绑定事件（针正有意义的事件）。
bb.Get<int>("key1").Value++;//自增，抛出事件，打印“callback”。

bb.Set("key2", true);
bb.Get<bool>("key2").Value;
```

注：在使用Set/UnSet方法的时候，也会触发事件（IsSet）

>> ## 时钟

提供回调事件的Timer。行为树并不会每分每秒Tick，在需要的时候注册Timer事件，不需要的时候注销。</br>
与黑板共同驱动了整个行为树。

注：与<https://github.com/akbiggs/UnityTimer>相似，但加入了对象池。

>> ## TODO

- [ ] 完善节点（包含断点）
- [ ] 完善Debug工具
- [ ] ？编辑器

>> ## 参考链接

NPBehave <https://github.com/meniku/NPBehave>

UE4 Doc <https://docs.unrealengine.com/zh-CN/Engine/ArtificialIntelligence/BehaviorTrees/index.html>
