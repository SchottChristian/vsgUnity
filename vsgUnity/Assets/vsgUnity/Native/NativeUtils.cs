﻿//----------------------------------------------
//            vsgUnity: Native
// Writen by Thomas Hogarth
// NativeUtils.cs
//----------------------------------------------

using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using UnityEditor;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Experimental.Rendering;

namespace vsgUnity.Native
{

    public static class Library
    {
        //public const string libraryName = "unity2vsgd";
        public const string libraryName = "unity2vsg";
    }

    //
    // Local Unity types, should match layout of types in unity2vg DataTypes.h, used to pass data from C# to native code
    //

    [StructLayout(LayoutKind.Sequential)]
    public struct ByteArray
    {
        public byte[] data;
        public int length;
    }

    [StructLayout(LayoutKind.Sequential)]
	public struct IntArray
	{
		public int[] data;
		public int length;
	}

	[StructLayout(LayoutKind.Sequential)]
	public struct FloatArray
	{
		public float[] data;
		public int length;
	}

	[StructLayout(LayoutKind.Sequential)]
	public struct Vec2Array
	{
		public Vector2[] data;
		public int length;
	}

    [StructLayout(LayoutKind.Sequential)]
    public struct Vec3Array
    {
        public Vector3[] data;
        public int length;
    }

    [StructLayout(LayoutKind.Sequential)]
    public struct Vec4Array
    {
        public Vector4[] data;
        public int length;
    }

    [StructLayout(LayoutKind.Sequential)]
    public struct ColorArray
    {
        public Color[] data;
        public int length;
    }

    [StructLayout(LayoutKind.Sequential)]
    public struct MeshData
    {
        public int id;
        public Vec3Array verticies;
        public IntArray triangles;
        public Vec3Array normals;
        public Vec3Array tangents;
        public ColorArray colors;
        public Vec2Array uv0;
        public Vec2Array uv1;
    }

    public enum TexFormat
    {
        R8_UNORM = 0,
        R8G8_UNORM = 1,
        R8G8B8_UNORM = 2,
        R8G8B8A8_UNORM = 3,
        BC1_RGB_UNORM = 4, //dxt1
        BC1_RGBA_UNORM = 5, //dxt1
        Unsupported = 9999
    }

    public enum MipmapFilterMode
    {
        Point = 0,
        Bilinear = 1,
        Trilinear = 2,
        Unsupported = 9999
    }

    public enum WrapMode
    {
        Repeat = 0,
        Clamp = 1,
        Mirror = 2,
        MirrorOnce = 3,
        Unsupported = 9999
    }

    [StructLayout(LayoutKind.Sequential)]
    public struct TextureData
    {
        public int id;
        public int channel;
        public ByteArray pixels;
        public TexFormat format;
        public int width;
        public int height;
        public int depth;
        public int anisoLevel;
        public WrapMode wrapMode;
        public MipmapFilterMode filterMode;
        public int mipmapCount;
        public float mipmapBias;
    }

    [StructLayout(LayoutKind.Sequential)]
    public struct PipelineData
    {
        public int id;
        public int hasNormals;
        public int hasTangents;
        public int hasColors;
        public int uvChannelCount;
        public int vertexImageSamplerCount;
        public int fragmentImageSamplerCount;
        public int vertexUniformCount;
        public int fragmentUniformCount;
    };

    [StructLayout(LayoutKind.Sequential)]
    public struct TransformData
    {
        public FloatArray matrix;
    }

    [StructLayout(LayoutKind.Sequential)]
    public struct CameraData
    {
        public Vector3 position;
        public Vector3 lookAt;
        public Vector3 upDir;
        public float fov;
        public float nearZ;
        public float farZ;
    }

    //
    // Native types for data returned from native code to C#
    //

    [StructLayout(LayoutKind.Sequential)]
	public struct NativeIntArray
	{
		public IntPtr ptr;
		public int length;
	}

	[StructLayout(LayoutKind.Sequential)]
	public struct NativeFloatArray
	{
		public IntPtr ptr;
		public int length;
	}

	[StructLayout(LayoutKind.Sequential)]
	public struct NativeVec2Array
	{
		public IntPtr ptr;
		public int length;
	}

    [StructLayout(LayoutKind.Sequential)]
    public struct NativeVec3Array
    {
        public IntPtr ptr;
        public int length;
    }

    [StructLayout(LayoutKind.Sequential)]
    public struct NativeVec4Array
    {
        public IntPtr ptr;
        public int length;
    }

    public static class NativeUtils
    {
        public static TexFormat GetTextureFormat(GraphicsFormat format)
        {
            switch(format)
            {
                case GraphicsFormat.R8_UNorm: return TexFormat.R8_UNORM;
                case GraphicsFormat.R8G8_UNorm: return TexFormat.R8G8_UNORM;
                case GraphicsFormat.R8G8B8A8_UNorm: return TexFormat.R8G8B8A8_UNORM;
                case GraphicsFormat.RGBA_DXT1_UNorm: return TexFormat.BC1_RGBA_UNORM;
                //case TextureFormat.DXT5: return TexFormat.BC1_RGBA_UNORM;
                default: break;
            }
            return TexFormat.Unsupported;
        }

        public static MipmapFilterMode GetTextureFilterMode(FilterMode filter)
        {
            switch(filter)
            {
                case FilterMode.Point: return MipmapFilterMode.Point;
                case FilterMode.Bilinear: return MipmapFilterMode.Bilinear;
                case FilterMode.Trilinear: return MipmapFilterMode.Trilinear;
                default: break;
            }
            return MipmapFilterMode.Unsupported;
        }

        public static WrapMode GetTextureWrapMode(TextureWrapMode wrap)
        {
            switch (wrap)
            {
                case TextureWrapMode.Repeat: return WrapMode.Repeat;
                case TextureWrapMode.Clamp: return WrapMode.Clamp;
                case TextureWrapMode.Mirror: return WrapMode.Mirror;
                case TextureWrapMode.MirrorOnce: return WrapMode.MirrorOnce;
                default: break;
            }
            return WrapMode.Unsupported;
        }

        //
        // Returns empty string if texture is support,
        // otherwise returns description of unsupported feature
        //
        public static string IsTextureSupported(Texture texture)
        {
            if (!texture.isReadable) return "Texture '" + texture.name + "' is not readable. Please enable Read/Write in import settings.";
            TexFormat format = GetTextureFormat(texture.graphicsFormat);
            if (format == TexFormat.Unsupported) return "Texture '" + texture.name + "' is using unsupported format '" + texture.graphicsFormat.ToString() + "'. Please select another format (RGBA32) in import settings";

            if(texture.dimension != TextureDimension.Tex2D && texture.dimension != TextureDimension.Tex3D) return "Texture '" + texture.name + "' is an unsupported dimension '" + texture.dimension.ToString() + "'. Please select another dimension (2D,3D) in import settings";

            return string.Empty;
        }

        public static TextureData CreateTextureData(Texture texture)
        {
            TextureData texdata = new TextureData();

            switch(texture.dimension)
            {
                case TextureDimension.Tex2D: PopulateTextureData(texture as Texture2D, ref texdata); break;
                case TextureDimension.Tex3D: PopulateTextureData(texture as Texture3D, ref texdata); break;
                default: break;
            }

            return texdata;
        }

        public static bool PopulateTextureData(Texture2D texture, ref TextureData texdata)
        {
            if (!PopulateTextureData(texture as Texture, ref texdata)) return false;
            texdata.depth = 1;
            texdata.pixels.data = texture.GetRawTextureData();
            texdata.pixels.length = texdata.pixels.data.Length;
            return true;
        }

        public static bool PopulateTextureData(Texture3D texture, ref TextureData texdata)
        {
            if (!PopulateTextureData(texture as Texture, ref texdata)) return false;
            texdata.depth = texture.depth;
            texdata.pixels.data = Color32ArrayToByteArray(texture.GetPixels32());
            texdata.pixels.length = texdata.pixels.data.Length;
            return true;
        }

        //
        // Populate the base data accesible via Texture, exludes pixel data and depth
        //
        public static bool PopulateTextureData(Texture texture, ref TextureData texdata)
        {
            texdata.id = texture.GetInstanceID();
            texdata.format = GetTextureFormat(texture.graphicsFormat);
            texdata.width = texture.width;
            texdata.height = texture.height;
            texdata.anisoLevel = texture.anisoLevel;
            texdata.wrapMode = GetTextureWrapMode(texture.wrapMode);
            texdata.filterMode = MipmapFilterMode.Point;
            texdata.mipmapCount = 0;
            texdata.mipmapBias = 0.0f;
            return true;
        }

        public static PipelineData CreatePipeLineData(MeshData meshData)
        {
            PipelineData pipeline = new PipelineData();
            pipeline.hasNormals = meshData.normals.length > 0 ? 1 : 0;
            pipeline.hasTangents = meshData.tangents.length > 0 ? 1 : 0;
            pipeline.hasColors = meshData.colors.length > 0 ? 1 : 0;
            pipeline.uvChannelCount = meshData.uv0.length > 0 ? 1 : 0;
            return pipeline;
        }

        public static CameraData CreateCameraData(Camera camera)
        {
            CameraData camdata = new CameraData();
            camdata.position = camera.gameObject.transform.position;
            camdata.lookAt = camdata.position + camera.gameObject.transform.forward;
            camdata.upDir = camera.gameObject.transform.up;
            camdata.fov = camera.fieldOfView;
            camdata.nearZ = camera.nearClipPlane;
            camdata.farZ = camera.farClipPlane;
            return camdata;
        }

        public static Dictionary<string, Texture> GetTexturesForMaterial(Material mat)
        {
            Dictionary<string, Texture> textures = new Dictionary<string, Texture>();

            Shader shader = mat.shader;
            for (int i = 0; i < ShaderUtil.GetPropertyCount(shader); i++)
            {
                if (ShaderUtil.GetPropertyType(shader, i) == ShaderUtil.ShaderPropertyType.TexEnv)
                {
                    string propname = ShaderUtil.GetPropertyName(shader, i);
                    Texture texture = mat.GetTexture(propname);
                    textures.Add(propname, texture);
                }
            }
            return textures;
        }

        private static byte[] Color32ArrayToByteArray(Color32[] colors)
        {
            if (colors == null || colors.Length == 0)
                return null;

            int lengthOfColor32 = Marshal.SizeOf(typeof(Color32));
            int length = lengthOfColor32 * colors.Length;
            byte[] bytes = new byte[length];

            GCHandle handle = default(GCHandle);
            try
            {
                handle = GCHandle.Alloc(colors, GCHandleType.Pinned);
                IntPtr ptr = handle.AddrOfPinnedObject();
                Marshal.Copy(ptr, bytes, 0, length);
            }
            finally
            {
                if (handle != default(GCHandle))
                    handle.Free();
            }

            return bytes;
        }
    }

    public static class Memory
	{
#if UNITY_IPHONE
		[DllImport ("__Internal")]
#else
        [DllImport(Library.libraryName, EntryPoint = "unity2vsg_DataTypes_DeleteNativeObject")]
#endif
        private static extern void unity2vsg_DataTypes_DeleteNativeObject(IntPtr anObjectPointer, bool isArray);

		public static void DeleteNativeObject(IntPtr anObjectPointer, bool isArray)
		{
            unity2vsg_DataTypes_DeleteNativeObject(anObjectPointer, isArray);
		}
	}

    public static class Convert
	{
	 	private static T[] CreateArray<T>(IntPtr array, int length)
		{
	         T[] result = new T[length];
	         int size = Marshal.SizeOf(typeof(T));
	 
	         if (IntPtr.Size == 4) {
	             // 32-bit system
	             for (int i = 0; i < result.Length; i++) {
	                 result [i] = (T)Marshal.PtrToStructure (array, typeof(T));
	                 array = new IntPtr (array.ToInt32 () + size);
	             }
	         } else {
	             // probably 64-bit system
	             for (int i = 0; i < result.Length; i++) {
	                 result [i] = (T)Marshal.PtrToStructure (array, typeof(T));
	                 array = new IntPtr(array.ToInt64 () + size);
	             }
	         }
	         return result;
     	}

		public static IntArray FromLocal(int[] anArray)
		{
			IntArray result;
			result.data = anArray;
			result.length = anArray.Length;
			return result;
		}

		public static FloatArray FromLocal(float[] anArray)
		{
			FloatArray result;
			result.data = anArray;
			result.length = anArray.Length;
			return result;
		}

		public static Vec2Array FromLocal(Vector2[] anArray)
		{
			Vec2Array result;
			result.data = anArray;
			result.length = anArray.Length;
			return result;
		}

        public static Vec3Array FromLocal(Vector3[] anArray)
        {
            Vec3Array result;
            result.data = anArray;
            result.length = anArray.Length;
            return result;
        }

        public static Vec4Array FromLocal(Vector4[] anArray)
        {
            Vec4Array result;
            result.data = anArray;
            result.length = anArray.Length;
            return result;
        }

        public static IntArray FromNative(NativeIntArray aNativeArray)
		{
			IntArray result;
			result.data = CreateArray<int>(aNativeArray.ptr, aNativeArray.length);
			result.length = result.data.Length;
			return result;
		}

		public static FloatArray FromNative(NativeFloatArray aNativeArray)
		{
			FloatArray result;
			result.data = CreateArray<float>(aNativeArray.ptr, aNativeArray.length);
			result.length = result.data.Length;
			return result;
		}

		public static Vec2Array FromNative(NativeVec2Array aNativeArray)
		{
			Vec2Array result;
			result.data = CreateArray<Vector2>(aNativeArray.ptr, aNativeArray.length);
			result.length = result.data.Length;
			return result;
		}

        public static Vec3Array FromNative(NativeVec3Array aNativeArray)
        {
            Vec3Array result;
            result.data = CreateArray<Vector3>(aNativeArray.ptr, aNativeArray.length);
            result.length = result.data.Length;
            return result;
        }

        public static Vec4Array FromNative(NativeVec4Array aNativeArray)
        {
            Vec4Array result;
            result.data = CreateArray<Vector4>(aNativeArray.ptr, aNativeArray.length);
            result.length = result.data.Length;
            return result;
        }
    }

}
