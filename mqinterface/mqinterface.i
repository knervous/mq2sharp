



%module MQ2Sharp

%include "mqignore.i"
%include "std_function.i"

%{

#include <string>
#include <list>
#include <any>
#include <set>
#include <vector>
#include <memory>
#include <iostream>
#include <mq/Plugin.h>
#include "../dotnet_runtime.h"
#include "mq/imgui/ConsoleWidget.h"
#include "main/MQ2ImGuiTools.h"

#include "imgui/imconfig.h"
#include "imgui/imgui_internal.h"
#include "imgui/imgui.h"
#include "imgui/ImGuiUtils.h"

#include "eqlib/Config.h"
#include "eqlib/ChatFilters.h"
#include "eqlib/PlayerClient.h"
#include "eqlib/ForwardDecls.h"
#include "eqlib/Items.h"
#include "eqlib/Spells.h"
#include "eqlib/EQClasses.h"
#include "eqlib/CXStr.h"
#include "eqlib/Constants.h"
#include "eqlib/Containers.h"
#include "eqlib/EverQuest.h"
#include "eqlib/GraphicsEngine.h"

%}

// %define eqlib::USERCOLOR_DEFAULT 273
// %enddef

%include <windows.i>


// Defines to be ignored by SWIG
#define CALLBACK
#define SIZE_CHECK(x,y)
#define FORCE_SYMBOLS
#define ALT_MEMBER_GETTER_DEPRECATED(type, orig, name, msg) \
    DEPRECATE(msg) \
    type& getter_ ## name() { return (*reinterpret_cast<type*>(&orig)); }
#define ALT_MEMBER_ALIAS_DEPRECATED(type, orig, name, msg) \
	DEPRECATE(msg) \
    type& getter_ ## name() { return orig; } \
    void setter_ ## name(const type& v) { orig = v; }

%include <std_list.i>
%include <std_except.i>
%include <std_vector.i>
%include <std_string.i>
%include <csharp/std_string.i>
%include <std_unordered_map.i>
%include <std_map.i>
%include <std_shared_ptr.i>
%include <stdint.i>
%include <std_string.i>
%include <typemaps.i>

%include "csharp.swg" // C# specific SWIG library

%apply uint32_t { typename std::make_unsigned<int>::type };

// Map bool* to out bool in C#



%inline %{
const char* type_name_int() {
    return "int";
}
const char* type_name_double() {
    return "double";
}
const char* type_name_string() {
    return "std::string";
}


namespace eqlib {
    ActorBase::~ActorBase() {
        // Implementation code
    }
}
void MQLog(const std::string& msg, int color, int filter) {
    mq::WriteChatColor(msg.c_str(), color, filter);
}


%}



// Mock the necessary fmt components for SWIG
namespace fmt {
    namespace v10 {
        template <typename T>
        struct formatter {
            // Add minimalistic functional content if necessary
            template <typename FormatContext>
            auto format(const T& value, FormatContext& ctx) -> decltype(ctx.out()) {
                return ctx.out(); // Stub implementation
            }
        };

        using string_view = const char*; // Simplify string_view for SWIG
    }
}

// Provide SWIG with the template specialization that it needs to see
template <>
struct fmt::v10::formatter<eqlib::CXStr> : fmt::v10::formatter<fmt::v10::string_view> {
    template <typename FormatContext>
    auto format(const eqlib::CXStr& s, FormatContext& ctx) -> decltype(ctx.out()) {
        return fmt::v10::formatter<fmt::v10::string_view>::format("Your string conversion here", ctx);
    }
};

%pragma(csharp) modulecode=%{
public delegate void fEQCommandConstChar(IntPtr client, IntPtr message);
%}

%typemap(cstype) fEQCommandConstChar "fEQCommandConstChar";
%typemap(imtype) fEQCommandConstChar "IntPtr";
%typemap(csimtype) fEQCommandConstChar "IntPtr";
%typemap(csin) fEQCommandConstChar "global::System.Runtime.InteropServices.Marshal.GetFunctionPointerForDelegate($csinput)"
%typemap(in) fEQCommandConstChar {
    $1 = (fEQCommandConstChar)$input;
}

%std_function(EQCmnd, void, eqlib::PlayerClient*, const char*);


%inline %{
std::function<void(eqlib::PlayerClient* , const char*)> make_eqcmd_func(fEQCommandConstChar cmd) {
    return [cmd](eqlib::PlayerClient* c, const char* y){
        cmd(c, y);
    };
}

%}


%include "../dotnet_runtime.h"
%include "mq/base/Common.h"
%include "mq/base/Deprecation.h"
%include "MacroDataTypesCopy.h"
%include "mq/api/MacroAPI.h"
%include "mq/api/Main.h"
%include "mq/api/Plugin.h"
%include "mq/imgui/ConsoleWidget.h"

%include "main/MQ2Main.h"
%include "main/MQ2Commands.h"
%include "main/MQ2Globals.h"
%include "main/MQ2ImGuiTools.h"
%include "main/MQ2Prototypes.h"
// %include "main/MQ2Internal.h"
%include "main/MQDataAPI.h"

%include "imgui/imconfig.h"
%include "imgui/imgui.h"
%include "imgui/ImGuiUtils.h"

%include "eqlib/Config.h"
%include "eqlib/ChatFilters.h"
%include "eqlib/PlayerClient.h"
%include "eqlib/ForwardDecls.h"
%include "eqlib/Items.h"
%include "eqlib/Spells.h"
%include "eqlib/CXStr.h"
%include "eqlib/Constants.h"
%include "eqlib/Expansions.h"
%include "eqlib/Containers.h"
%include "eqlib/EverQuest.h"
%include "eqlib/GraphicsEngine.h"
%include "eqlib/EQClasses.h"

%include cpointer.i
%pointer_functions(bool, boolp);

