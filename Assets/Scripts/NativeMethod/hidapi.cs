using UnityEngine;
using System.Runtime.InteropServices;
using System.Collections;
using System;
using System.Text;

/*
 HIDAPI - Multi-Platform library for
 communication with HID devices.

 Copyright 2009, Alan Ott, Signal 11 Software.
 All Rights Reserved.
 
 This software may be used by anyone for any reason so
 long as the copyright notice in the source files
 remains intact.
*/

/*
The MIT License (MIT)

Copyright (c) 2015 Adrian Biagioli

Permission is hereby granted, free of charge, to any person obtaining a copy
of this software and associated documentation files (the "Software"), to deal
in the Software without restriction, including without limitation the rights
to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
copies of the Software, and to permit persons to whom the Software is
furnished to do so, subject to the following conditions:

The above copyright notice and this permission notice shall be included in all
copies or substantial portions of the Software.

THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE
SOFTWARE.
*/


public class HIDapi
{

    [DllImport("hidapi")]
    public static extern int hid_init();

    [DllImport("hidapi")]
    public static extern int hid_exit();

    [DllImport("hidapi")]
    public static extern IntPtr hid_error(IntPtr device);

    [DllImport("hidapi")]
    public static extern IntPtr hid_enumerate(ushort vendor_id, ushort product_id);

    [DllImport("hidapi")]
    public static extern void hid_free_enumeration(IntPtr devs);

    [DllImport("hidapi")]
    public static extern int hid_get_feature_report(IntPtr device, byte[] data, UIntPtr length);

    [DllImport("hidapi")]
    public static extern int hid_get_indexed_string(IntPtr device, int string_index, StringBuilder str, UIntPtr maxlen);

    [DllImport("hidapi")]
    public static extern int hid_get_manufacturer_string(IntPtr device, StringBuilder str, UIntPtr maxlen);

    [DllImport("hidapi")]
    public static extern int hid_get_product_string(IntPtr device, StringBuilder str, UIntPtr maxlen);

    [DllImport("hidapi")]
    public static extern int hid_get_serial_number_string(IntPtr device, StringBuilder str, UIntPtr maxlen);

    [DllImport("hidapi")]
    public static extern IntPtr hid_open(ushort vendor_id, ushort product_id, string serial_number);

    [DllImport("hidapi")]
    public static extern void hid_close(IntPtr device);

    [DllImport("hidapi")]
    public static extern IntPtr hid_open_path(string path);

    [DllImport("hidapi")]
    public static extern int hid_read(IntPtr device, byte[] data, ulong length);

    [DllImport("hidapi")]
    public static extern int hid_read_timeout(IntPtr dev, byte[] data, ulong length, int milliseconds);

    [DllImport("hidapi")]
    public static extern int hid_send_feature_report(IntPtr device, byte[] data, uint length);

    [DllImport("hidapi")]
    public static extern int hid_set_nonblocking(IntPtr device, int nonblock);

    [DllImport("hidapi")]
    public static extern int hid_write(IntPtr device, byte[] data, uint length);
}

struct hid_device_info
{
    public string path;
    public ushort vendor_id;
    public ushort product_id;
    public IntPtr serial_number;
    public ushort release_number;
    public string manufacturer_string;
    public IntPtr product_string;
    public ushort usage_page;
    public ushort usage;
    public int interface_number;
    public IntPtr next;
}