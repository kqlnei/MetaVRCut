using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public class UnsafeList<T> : IEnumerable<T> //List.Add()���x���̂�List����z�����������o���Ē��ړ��͂��邽�߂Ɏg�p.
{
    public T[] unsafe_array;
    int capacity;
    public int unsafe_count;
    public int Count { get { return unsafe_count; } }//unsafe_count�ƈ���Ĉ��S
    public int Capacity { get { return capacity; } }//�����z��̍ő���e��(�v�f��������ɑ���)

    public UnsafeList(int cap)
    {
        if (cap <= 0) { cap = 1; }
        unsafe_array = new T[cap];
        capacity = cap;
        unsafe_count = 0;
    }
    public T this[int index]
    {
        get
        {
            if (index >= unsafe_count) { Debug.LogError("index is out of range!!"); }
            return unsafe_array[index];
        }
        set
        {
            if (index >= unsafe_count) { Debug.LogError("index is out of range!!"); }
            unsafe_array[index] = value;
        }
    }


    public void Add(T value)
    {
        if (capacity == unsafe_count)//�z�񂪖��܂�����V�����z��Ɉڂ��ւ���
        {
            capacity = (capacity) * 2;
            var temp = new T[capacity];
            Array.Copy(unsafe_array, temp, unsafe_count);
            unsafe_array = temp;
        }
        unsafe_array[unsafe_count++] = value;
    }

    public UnsafeList<T> Clear(int _minCapacity = 20)//�������Ɠ����Ɋg��
    {
        if (capacity < _minCapacity)
        {
            var temp = new T[_minCapacity];
            unsafe_array = temp;
            capacity = _minCapacity;
        }
        unsafe_count = 0;
        return this;
    }

    public T[] ToArray()
    {
        var output = new T[unsafe_count];
        Array.Copy(unsafe_array, output, unsafe_count);
        return output;
    }

    public List<T> ToList()
    {
        var output = new T[unsafe_count];
        Array.Copy(unsafe_array, output, unsafe_count);
        return new List<T>(output);
    }

    public void AddOnlyCount()//�J�E���g�������₵�ĈȑO�Ɏg���Ă������̂����̂܂ܓ����Ă��Ԃɂ���(�N���X�̎g���܂킵�ɗ��p)
    {
        if (capacity == unsafe_count)
        {
            capacity = (capacity) * 2;
            var temp = new T[capacity];
            Array.Copy(unsafe_array, temp, unsafe_count);
            unsafe_array = temp;
        }
        unsafe_count++;
    }

    public T Top //list�̐擪��Ԃ�
    {
        get
        {
            return unsafe_array[unsafe_count - 1];
        }
        set
        {
            unsafe_array[unsafe_count - 1] = value;
        }
    }


    //�������牺��foreach�ŉ񂹂�悤��IEnumerable�̎��������Ă���
    public IEnumerator<T> GetEnumerator() { return new UnsafeListEnumerator(unsafe_array, unsafe_count); }
    IEnumerator IEnumerable.GetEnumerator() { return this.GetEnumerator(); }

    class UnsafeListEnumerator : IEnumerator<T>
    {
        int index;
        int arrayLength;
        T[] array;

        public T Current { get { return array[index]; } }
        object IEnumerator.Current { get { return array[index]; } }

        public UnsafeListEnumerator(T[] array, int arrayLength)
        {
            index = -1;
            this.arrayLength = arrayLength;
            this.array = array;
        }

        public bool MoveNext()
        {
            if (++index >= arrayLength)
            {
                return false;
            }

            return true;
        }
        public void Reset()
        {
            index = -1;
        }
        void IDisposable.Dispose() { }
    }
}
