#region Header
/**
 * 名称: 游戏对象池中的游戏对象
 * 描述：
 *      1.维护了m_isInPool,多处对一个游戏对象放回池中的处理时会报错
 **/
#endregion
using UnityEngine;
using System.Collections;
using System.Collections.Generic;


public class GameObjectPoolObject : MonoBehaviour
{
    #region Fields
    public System.Func<bool> m_onIngoreDestroy;//无视销毁操作的接口，用于某些特殊的特效做延迟销毁
    bool m_isInPool;

    #endregion


    #region Properties
    public bool IsInPool {
        get { return m_isInPool;}
    }

    #endregion

    #region Static Methods

    #endregion


    #region Mono Frame
    void OnEnable()
    {

    }

    void OnDiable()
    {

    }
    #endregion
   
    #region frame
    public void OnInit()
    {
        m_isInPool = false;
    }

    public void OnPut()
    {
        if (m_isInPool) 
            Debuger.LogError("已经在对象池中");//检错下
        m_isInPool = true;
    }

    public void OnGet()
    {
        if (!m_isInPool) 
            Debuger.LogError("不在在对象池中");//检错下
        m_isInPool = false;

    }

#endregion

#region Private Methods
    
#endregion
    
}
