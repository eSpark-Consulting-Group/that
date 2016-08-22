#pragma once

#include <functional>

template<class T> class CAutoHandle
{
public:
	typedef std::function<void(const T&)> fin_t;

	CAutoHandle(fin_t finalizer) :
		_value(0),
		_finalizer(finalizer)
	{ }

	CAutoHandle(const T& value, fin_t finalizer) :
		_value(value),
		_finalizer(finalizer)
	{ }

	// Special case for memory allocations
	CAutoHandle(size_t allocSize)
	{
		_value = (T)calloc(1, allocSize),
			_finalizer = [](const T& p) { free(p); };
	}

	~CAutoHandle()
	{
		if (_finalizer && _value)
			_finalizer(_value);
	}

	void Release()
	{
		if (_finalizer && _value)
		{
			_finalizer(_value);
			_value = 0;
		}
	}

	operator const T&() const { return _value; }

	T& operator=(const T& rvalue)
	{
		if (_value != rvalue)
		{
			if (_finalizer && _value)
				_finalizer(_value);
			_value = rvalue;
		}
		return _value;
	}

	T& operator->() { return _value; }

	T* operator&()
	{
		ATLASSERT(_value == 0);
		return &_value;
	}

private:
	T _value;
	fin_t _finalizer;
};
