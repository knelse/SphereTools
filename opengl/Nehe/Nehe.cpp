#include <windows.h>
#include <gl/glut.h>
#include <tchar.h>

HGLRC h_rc = nullptr;
HDC h_dc = nullptr;
HWND h_wnd = nullptr;
HINSTANCE h_instance;

bool g_keys[256];
bool g_is_active = true;
bool g_is_fullscreen = true;

LRESULT CALLBACK wnd_proc(HWND, UINT, WPARAM, LPARAM);

GLvoid resize_gl_scene(const GLsizei width, GLsizei height) {
	if (height == 0) height = 1;

	glViewport(0, 0, width, height);

	glMatrixMode(GL_PROJECTION);
	glLoadIdentity();

	gluPerspective(45.0f, static_cast<GLfloat>(width) / static_cast<GLfloat>(height), 0.1f, 100.0f);

	glMatrixMode(GL_MODELVIEW);
	glLoadIdentity();
}

int init_gl() {
	glShadeModel(GL_SMOOTH);
	glClearColor(1.0f, 1.0f, 1.0f, 1.0f);
	glClearDepth(1.0f);
	glEnable(GL_DEPTH_TEST);
	glDepthFunc(GL_LEQUAL);

	glHint(GL_PERSPECTIVE_CORRECTION_HINT, GL_NICEST);

	return true;
}

int draw_gl_scene() {
	glClear(GL_COLOR_BUFFER_BIT | GL_DEPTH_BUFFER_BIT);
	glLoadIdentity();
	return true;
}

GLvoid kill_gl_window() {
	if (g_is_fullscreen) {
		ChangeDisplaySettings(nullptr, 0);
		ShowCursor(true);
	}
	if (h_rc) {
		if (!wglMakeCurrent(nullptr, nullptr)) MessageBox(nullptr, (LPCSTR)"DC/RC release failed", (LPCSTR)"SHUTDOWN ERROR", MB_OK | MB_ICONINFORMATION);
		if (!wglDeleteContext(h_rc)) {
			MessageBox(nullptr, (LPCSTR)"Rendering context release failed", (LPCSTR)"SHUTDOWN ERROR", MB_OK | MB_ICONINFORMATION);
			h_rc = nullptr;
		}
		if (h_dc && !ReleaseDC(h_wnd, h_dc)) {
			MessageBox(nullptr, (LPCSTR)"Device context release failed", (LPCSTR)"SHUTDOWN ERROR", MB_OK | MB_ICONINFORMATION);
			h_dc = nullptr;
		}
		if (h_wnd && !DestroyWindow(h_wnd)) {
			MessageBox(nullptr, (LPCSTR)"hWnd release failed", (LPCSTR)"SHUTDOWN ERROR", MB_OK | MB_ICONINFORMATION);
			h_wnd = nullptr;
		}
		if (!UnregisterClass((LPCSTR)"OpenGL", h_instance)) {
			MessageBox(nullptr, (LPCSTR)"Class unregister failed", (LPCSTR)"SHUTDOWN ERROR", MB_OK | MB_ICONINFORMATION);
			h_instance = nullptr;
		}
	}
}

bool create_gl_window(const char* title, const int width, const int height, const int bits, const bool fullscreen_flag) {
	GLuint pixel_format;
	WNDCLASS wc;
	DWORD dw_ex_style;
	DWORD dw_style;
	RECT window_rect;

	window_rect.left = static_cast<long>(0);
	window_rect.right = static_cast<long>(width);
	window_rect.top = static_cast<long>(0);
	window_rect.bottom = static_cast<long>(height);
	g_is_fullscreen = fullscreen_flag;

	h_instance = GetModuleHandle(nullptr);
	wc.style = CS_HREDRAW | CS_VREDRAW | CS_OWNDC;
	wc.lpfnWndProc = static_cast<WNDPROC>(wnd_proc);
	wc.cbClsExtra = 0;
	wc.cbWndExtra = 0;
	wc.hInstance = h_instance;
	wc.hIcon = LoadIcon(nullptr, IDI_ASTERISK);
	wc.hCursor = LoadCursor(nullptr, IDC_ARROW);
	wc.hbrBackground = nullptr;
	wc.lpszMenuName = nullptr;
	wc.lpszClassName = (LPCSTR)"OpenGL";

	if (!RegisterClass(&wc)) {
		MessageBox(nullptr, (LPCSTR)"Class register failed", (LPCSTR)"SHUTDOWN ERROR", MB_OK | MB_ICONEXCLAMATION);
		return false;
	}

	if (g_is_fullscreen) {
		DEVMODE dm_screen_settings = {};
		dm_screen_settings.dmSize = sizeof(dm_screen_settings);
		dm_screen_settings.dmPelsWidth = width;
		dm_screen_settings.dmPelsHeight = height;
		dm_screen_settings.dmBitsPerPel = bits;
		dm_screen_settings.dmFields = DM_BITSPERPEL | DM_PELSWIDTH | DM_PELSHEIGHT;

		if (ChangeDisplaySettings(&dm_screen_settings, CDS_FULLSCREEN) != DISP_CHANGE_SUCCESSFUL)
		{
			if (MessageBox(nullptr, (LPCSTR)"Requested fullscreen is not supported by your video card, use window mode instead?", (LPCSTR)"OpenGL", MB_YESNO | MB_ICONEXCLAMATION) != IDYES)
			{
				MessageBox(nullptr, (LPCSTR)"Program will now close", (LPCSTR)"ERROR", MB_OK | MB_ICONSTOP);
				return false;
			}
			g_is_fullscreen = false;
		}
	}
	if (g_is_fullscreen) {
		dw_ex_style = WS_EX_APPWINDOW;
		dw_style = WS_POPUP;
		ShowCursor(false);
	}
	else {
		dw_ex_style = WS_EX_APPWINDOW | WS_EX_WINDOWEDGE;
		dw_style = WS_OVERLAPPEDWINDOW;
	}
	AdjustWindowRectEx(&window_rect, dw_style, false, dw_ex_style);
	if (!((h_wnd = CreateWindowEx(dw_ex_style, (LPCSTR)"OpenGL", (LPCSTR)title, dw_style | WS_CLIPSIBLINGS | WS_CLIPCHILDREN,
	                             0, 0, window_rect.right - window_rect.left, window_rect.bottom - window_rect.top, nullptr, nullptr, h_instance, nullptr)))) {
		kill_gl_window();
		MessageBox(nullptr, (LPCSTR)"Window creation failed", (LPCSTR)"ERROR", MB_OK | MB_ICONEXCLAMATION);
		return false;
	}

	static PIXELFORMATDESCRIPTOR pfd = {
		sizeof(PIXELFORMATDESCRIPTOR),
		1,
		PFD_DRAW_TO_WINDOW | PFD_SUPPORT_OPENGL | PFD_DOUBLEBUFFER, PFD_TYPE_RGBA,
		static_cast<byte>(bits), 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 16, 0, 0, PFD_MAIN_PLANE, 0, 0, 0, 0
	};

	if (!((h_dc = GetDC(h_wnd)))) {
		kill_gl_window();
		MessageBox(nullptr, (LPCSTR)"GL device context creation failed", (LPCSTR)"ERROR", MB_OK | MB_ICONEXCLAMATION);
		return false;
	}

	if (!((pixel_format = ChoosePixelFormat(h_dc, &pfd)))) {
		kill_gl_window();
		MessageBox(nullptr, (LPCSTR)"Pixel format not found", (LPCSTR)"ERROR", MB_OK | MB_ICONEXCLAMATION);
		return false;
	}
	if (!SetPixelFormat(h_dc, pixel_format, &pfd)) {
		kill_gl_window();
		MessageBox(nullptr, (LPCSTR)"Pixel format setting failed", (LPCSTR)"ERROR", MB_OK | MB_ICONEXCLAMATION);
		return false;
	}

	if (!((h_rc = wglCreateContext(h_dc)))) {
		kill_gl_window();
		MessageBox(nullptr, (LPCSTR)"Rendering context creation failed", (LPCSTR)"ERROR", MB_OK | MB_ICONEXCLAMATION);
		return false;
	}
	if (!wglMakeCurrent(h_dc, h_rc)) {
		kill_gl_window();
		MessageBox(nullptr, (LPCSTR)"Rendering context activation failed", (LPCSTR)"ERROR", MB_OK | MB_ICONEXCLAMATION);
		return false;
	}

	ShowWindow(h_wnd, SW_SHOW);
	SetForegroundWindow(h_wnd);
	SetFocus(h_wnd);
	resize_gl_scene(width, height);

	if (!init_gl()) {
		kill_gl_window();
		MessageBox(nullptr, (LPCSTR)"Init failed", (LPCSTR)"ERROR", MB_OK | MB_ICONEXCLAMATION);
		return false;
	}

	return true;
}

LRESULT CALLBACK wnd_proc(const HWND hWnd, const UINT uMsg, const WPARAM wParam, const LPARAM lParam) {
	switch (uMsg) {
	case WM_ACTIVATE:
	{
		if (!HIWORD(wParam)) g_is_active = true;
		else g_is_active = false;
		return 0;
	}
	case WM_SYSCOMMAND:
	{
		switch (wParam) {
		case SC_SCREENSAVE:
		case SC_MONITORPOWER:
			return 0;
		default: ;
		}
		break;
	}
	case WM_CLOSE:
	{
		PostQuitMessage(0);
		return 0;
	}
	case WM_KEYDOWN:
	{
		g_keys[wParam] = true;
		return 0;
	}
	case WM_KEYUP:
	{
		g_keys[wParam] = false;
		return 0;
	}
	case WM_SIZE:
	{
		resize_gl_scene(LOWORD(lParam), HIWORD(lParam));
		return 0;
	}
	default: return DefWindowProc(hWnd, uMsg, wParam, lParam);
	}
	return DefWindowProc(hWnd, uMsg, wParam, lParam);
}

int WINAPI WinMain(
	_In_ HINSTANCE hInstance,
	_In_opt_ HINSTANCE hPrevInstance,
	_In_ LPSTR     lpCmdLine,
	_In_ int       nCmdShow
)
{
	MSG msg;
	bool done = false;

	if (MessageBox(nullptr, (LPCSTR)"Launch in full screen?", (LPCSTR)"Startup fullscreen", MB_YESNO | MB_ICONQUESTION) == IDNO) {
		g_is_fullscreen = false;
	}

	if (!create_gl_window(_T("OpenGL"), 1920, 1080, 32, g_is_fullscreen)) return 0;
	while (!done) {
		if (PeekMessage(&msg, nullptr, 0, 0, PM_REMOVE))
		{
			if (msg.message == WM_QUIT) done = true;
			else {
				TranslateMessage(&msg);
				DispatchMessage(&msg);
			}
		}
		else
		{
			if (g_is_active) {
				if (g_keys[VK_ESCAPE]) done = true;
				if (g_keys[VK_F1]) {
					g_keys[VK_F1] = false;
					kill_gl_window();
					g_is_fullscreen = !g_is_fullscreen;
					if (!create_gl_window(_T("OpenGL"), 1920, 1080, 32, g_is_fullscreen)) return 0;
				}
				else
				{
					draw_gl_scene();
					SwapBuffers(h_dc);
				}
			}
		}
	}
	kill_gl_window();
	return (msg.wParam);
}