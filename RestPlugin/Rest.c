
#include <stdio.h>
#include <string.h>
#include <Windows.h>
#include <memory.h>
#include "XPLMDataAccess.h"
#include "XPLMProcessing.h"

// Shared memory for commands
static LPVOID gLpvCommandMem = NULL;
static HANDLE gHMapObjectCommand = NULL;
static HANDLE gHNewCommandEvent = NULL;

// Shared memory for replies
static LPVOID gLpvReplyMem = NULL;
static HANDLE gHMapObjectReply = NULL;
static HANDLE gHNewReplyEvent = NULL;

static HANDLE gHCommandThread = NULL;
static DWORD gDWCommandThreadId = NULL;

static BOOL gExitThreads = FALSE;

#define INTVAL		0x00
#define FLOATVAL	0x01
#define DOUBLEVAL	0x02
#define CHARVAL		0x03

#define QUERYREAD		0x00
#define QUERYWRITE		0x01
#define QUERYRESPONSE	0x02

typedef struct XplaneQuery
{
	char DataRefName[128];
	char DataRefType;
	char QueryType;
	char ValueCount;
	BOOL TreatAsArray;
	char ByteValues[500];
	int IntValues[256];
	float FloatValues[256];
	double DoubleValues[256];
} QUERY;

BOOL CreateSharedMemorySpace(void);

DWORD WINAPI HandleIncomingCommands(LPVOID lpParam);

BOOL CreateSharedMemorySpace()
{
	UINT SharedMemorySizeCommand = sizeof(QUERY);
	UINT SharedMemorySizeReply = sizeof(QUERY);

	gHMapObjectCommand = CreateFileMapping( 
                INVALID_HANDLE_VALUE,		// use paging file
                NULL,						// default security attributes
                PAGE_READWRITE,				// read/write access
                0,							// size: high 32-bits
                SharedMemorySizeCommand,    // size: low 32-bits
                TEXT("SHAREDMEM_COMMAND")); // name of map object

	gHMapObjectReply = CreateFileMapping( 
                INVALID_HANDLE_VALUE,		// use paging file
                NULL,						// default security attributes
                PAGE_READWRITE,				// read/write access
                0,							// size: high 32-bits
                SharedMemorySizeReply,    // size: low 32-bits
                TEXT("SHAREDMEM_RESPONSE")); // name of map object

	gHNewCommandEvent = CreateEvent(NULL, TRUE, FALSE, TEXT("SHAREDMEM_COMMAND_EVENT"));
	gHNewReplyEvent = CreateEvent(NULL, TRUE, FALSE, TEXT("SHAREDMEM_RESPONSE_EVENT"));

	if (gHMapObjectCommand == NULL) 
                return FALSE; 

	gLpvCommandMem = MapViewOfFile( 
                gHMapObjectCommand, // object to map view of
                FILE_MAP_WRITE,		// read/write access
                0,					// high offset:  map from
                0,					// low offset:   beginning
                0);					// default: map entire file

	gLpvReplyMem = MapViewOfFile( 
				gHMapObjectReply, // object to map view of
                FILE_MAP_WRITE,		// read/write access
                0,					// high offset:  map from
                0,					// low offset:   beginning
                0);					// default: map entire file

	if ( gLpvCommandMem == NULL) 
                return FALSE; 

	// Init shared mem to 0
	memset(gLpvCommandMem, '\0', SharedMemorySizeCommand);
	memset(gLpvReplyMem, '\0', SharedMemorySizeReply);

	return TRUE;
}

DWORD WINAPI HandleIncomingCommands(LPVOID lpParam)
{
	DWORD waitResult;
	QUERY response;
	XPLMDataRef dataRef;
	
	while(gExitThreads!=TRUE)
	{
		waitResult = WaitForSingleObject(gHNewCommandEvent, 1000);
		if(waitResult != WAIT_TIMEOUT)
		{
			// Get new struct from shared memory
			QUERY newCommand;
			ResetEvent(gHNewCommandEvent);
			memcpy(&newCommand, gLpvCommandMem, sizeof(QUERY));
			dataRef = XPLMFindDataRef(newCommand.DataRefName);

			switch(newCommand.QueryType)
			{
			case QUERYREAD:
				response = newCommand;				
				switch(newCommand.DataRefType)
				{
				case INTVAL:
					if(response.ValueCount > 1)
						XPLMGetDatavi(dataRef, response.IntValues, 0, response.ValueCount);
					else
						response.IntValues[0] = XPLMGetDatai(dataRef);
					break;
				case FLOATVAL:
					if(response.ValueCount > 1)
						XPLMGetDatavf(dataRef, response.FloatValues, 0, response.ValueCount);
					else
						response.FloatValues[0] = XPLMGetDataf(dataRef);
					break;
				case DOUBLEVAL:
					response.DoubleValues[0] = XPLMGetDatad(dataRef);
					break;
				case CHARVAL:
					XPLMGetDatab(dataRef, response.ByteValues, 0, response.ValueCount);
					break;
				default: 
					break;
				}
				memcpy(gLpvReplyMem, &response, sizeof(QUERY));
				SetEvent(gHNewReplyEvent);
				break;
			case QUERYWRITE:
				switch(newCommand.DataRefType)
				{
				case INTVAL:
					if(newCommand.TreatAsArray)
						XPLMSetDatavi(dataRef, newCommand.IntValues, 0, newCommand.ValueCount);
					else
						XPLMSetDatai(dataRef, newCommand.IntValues[0]);
					break;
				case FLOATVAL:
					if(newCommand.TreatAsArray)
						XPLMSetDatavf(dataRef, newCommand.FloatValues, 0, newCommand.ValueCount);
					else
						XPLMSetDataf(dataRef, newCommand.FloatValues[0]);
					break;
				case DOUBLEVAL:
					XPLMSetDatad(dataRef, newCommand.DoubleValues[0]);
					break;
				case CHARVAL:
					XPLMSetDatab(dataRef, newCommand.ByteValues, 0, newCommand.ValueCount);
					break;
				default: 
					break;
				}
				
				break;
			case QUERYRESPONSE:
				// No responses are expacted on the plugin side
				break;
			default:
				break;
			}

			
		}
	}
	return 1;
}

PLUGIN_API int XPluginStart(
		char * outName,
		char * outSig,
		char * outDesc)
{
	// Plugin description
	strcpy(outName, "RESTful API Interface");
	strcpy(outSig, "oneios.client.rest");
	strcpy(outDesc, "Plugin exposes API onto shared memory for use via REST service.");

	// Prepare Shared Memory
	CreateSharedMemorySpace();

	gHCommandThread = CreateThread(
		NULL,
		0,
		HandleIncomingCommands,
		NULL,
		0,
		&gDWCommandThreadId);
}

PLUGIN_API void	XPluginStop(void)
{
	gExitThreads = TRUE;
	WaitForSingleObject(gHCommandThread, INFINITE);
	UnmapViewOfFile(gLpvCommandMem);
	CloseHandle(gHMapObjectCommand);
}             

