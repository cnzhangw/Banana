using System;
using System.Collections.Generic;
using System.Text;

namespace Banana
{
    /// <summary>
    /// 字符串相似度比较
    /// </summary>
    public class StringCompute
    {
        #region 私有变量
        /// <summary>
        /// 字符串1
        /// </summary>
        private char[] _ArrChar1;
        /// <summary>
        /// 字符串2
        /// </summary>
        private char[] _ArrChar2;
        /// <summary>
        /// 统计结果
        /// </summary>
        private Result _Result;
        /// <summary>
        /// 开始时间
        /// </summary>
        private DateTime _BeginTime;
        /// <summary>
        /// 结束时间
        /// </summary>
        private DateTime _EndTime;
        /// <summary>
        /// 计算次数
        /// </summary>
        private int _ComputeTimes;
        /// <summary>
        /// 算法矩阵
        /// </summary>
        private int[,] _Matrix;
        /// <summary>
        /// 矩阵列数
        /// </summary>
        private int _Column;
        /// <summary>
        /// 矩阵行数
        /// </summary>
        private int _Row;
        #endregion
        
        public StringCompute() { }
        
        #region 算法实现
        /// <summary>
        /// 初始化算法基本信息
        /// </summary>
        /// <param name="str1">字符串1</param>
        /// <param name="str2">字符串2</param>
        private void StringComputeInit(string str1, string str2)
        {
            _ArrChar1 = str1.ToCharArray();
            _ArrChar2 = str2.ToCharArray();
            _Result = new Result();
            _ComputeTimes = 0;
            _Row = _ArrChar1.Length + 1;
            _Column = _ArrChar2.Length + 1;
            _Matrix = new int[_Row, _Column];
        }
        /// <summary>
        /// 计算相似度
        /// </summary>
        private Result Compute()
        {
            //开始时间
            _BeginTime = DateTime.Now;
            //初始化矩阵的第一行和第一列
            this.InitMatrix();
            int intCost = 0;
            for (int i = 1; i < _Row; i++)
            {
                for (int j = 1; j < _Column; j++)
                {
                    if (_ArrChar1[i - 1] == _ArrChar2[j - 1])
                    {
                        intCost = 0;
                    }
                    else
                    {
                        intCost = 1;
                    }
                    //关键步骤，计算当前位置值为左边+1、上面+1、左上角+intCost中的最小值 
                    //循环遍历到最后_Matrix[_Row - 1, _Column - 1]即为两个字符串的距离
                    _Matrix[i, j] = this.Minimum(_Matrix[i - 1, j] + 1, _Matrix[i, j - 1] + 1, _Matrix[i - 1, j - 1] + intCost);
                    _ComputeTimes++;
                }
            }
            //结束时间
            _EndTime = DateTime.Now;
            //相似率 移动次数小于最长的字符串长度的20%算同一题
            int intLength = _Row > _Column ? _Row : _Column;

            _Result.Rate = (1 - (decimal)_Matrix[_Row - 1, _Column - 1] / intLength);
            _Result.UseTime = (_EndTime - _BeginTime).ToString();
            _Result.ComputeTimes = _ComputeTimes.ToString();
            _Result.Difference = _Matrix[_Row - 1, _Column - 1];
            return _Result;
        }

        /// <summary>
        /// 计算相似度（不记录比较时间）
        /// </summary>
        private Result SpeedyCompute()
        {
            //开始时间
            //_BeginTime = DateTime.Now;
            //初始化矩阵的第一行和第一列
            this.InitMatrix();
            int intCost = 0;
            for (int i = 1; i < _Row; i++)
            {
                for (int j = 1; j < _Column; j++)
                {
                    if (_ArrChar1[i - 1] == _ArrChar2[j - 1])
                    {
                        intCost = 0;
                    }
                    else
                    {
                        intCost = 1;
                    }
                    //关键步骤，计算当前位置值为左边+1、上面+1、左上角+intCost中的最小值 
                    //循环遍历到最后_Matrix[_Row - 1, _Column - 1]即为两个字符串的距离
                    _Matrix[i, j] = this.Minimum(_Matrix[i - 1, j] + 1, _Matrix[i, j - 1] + 1, _Matrix[i - 1, j - 1] + intCost);
                    _ComputeTimes++;
                }
            }
            //结束时间
            //_EndTime = DateTime.Now;
            //相似率 移动次数小于最长的字符串长度的20%算同一题
            int intLength = _Row > _Column ? _Row : _Column;

            _Result.Rate = (1 - (decimal)_Matrix[_Row - 1, _Column - 1] / intLength);
            // _Result.UseTime = (_EndTime - _BeginTime).ToString();
            _Result.ComputeTimes = _ComputeTimes.ToString();
            _Result.Difference = _Matrix[_Row - 1, _Column - 1];
            return _Result;
        }

        /// <summary>
        /// 计算相似度
        /// </summary>
        /// <param name="str1">字符串1</param>
        /// <param name="str2">字符串2</param>
        public Result Compute(string str1, string str2)
        {
            this.StringComputeInit(str1, str2);
            return this.Compute();
        }

        /// <summary>
        /// 计算相似度
        /// </summary>
        /// <param name="str1">字符串1</param>
        /// <param name="str2">字符串2</param>
        public Result SpeedyCompute(string str1, string str2)
        {
            this.StringComputeInit(str1, str2);
            return this.SpeedyCompute();
        }
        /// <summary>
        /// 初始化矩阵的第一行和第一列
        /// </summary>
        private void InitMatrix()
        {
            for (int i = 0; i < _Column; i++)
            {
                _Matrix[0, i] = i;
            }
            for (int i = 0; i < _Row; i++)
            {
                _Matrix[i, 0] = i;
            }
        }
        /// <summary>
        /// 取三个数中的最小值
        /// </summary>
        /// <param name="First"></param>
        /// <param name="Second"></param>
        /// <param name="Third"></param>
        /// <returns></returns>
        private int Minimum(int First, int Second, int Third)
        {
            int intMin = First;
            if (Second < intMin)
            {
                intMin = Second;
            }
            if (Third < intMin)
            {
                intMin = Third;
            }
            return intMin;
        }
        #endregion

        /// <summary>
        /// 计算结果
        /// </summary>
        public struct Result
        {
            /// <summary>
            /// 相似度
            /// </summary>
            public decimal Rate;
            /// <summary>
            /// 对比次数
            /// </summary>
            public string ComputeTimes;
            /// <summary>
            /// 使用时间
            /// </summary>
            public string UseTime;
            /// <summary>
            /// 差异
            /// </summary>
            public int Difference;
        }
    }
}