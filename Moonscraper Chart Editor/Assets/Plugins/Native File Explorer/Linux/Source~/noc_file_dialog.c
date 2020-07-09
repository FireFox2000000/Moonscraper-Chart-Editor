/* noc_file_dialog library
 *
 * Copyright (c) 2015 Guillaume Chereau <guillaume@noctua-software.com>
 *
 * Permission is hereby granted, free of charge, to any person obtaining a copy
 * of this software and associated documentation files (the "Software"), to
 * deal in the Software without restriction, including without limitation the
 * rights to use, copy, modify, merge, publish, distribute, sublicense, and/or
 * sell copies of the Software, and to permit persons to whom the Software is
 * furnished to do so, subject to the following conditions:
 *
 * The above copyright notice and this permission notice shall be included in
 * all copies or substantial portions of the Software.
 *
 * THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
 * IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
 * FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
 * AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
 * LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING
 * FROM, OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS
 * IN THE SOFTWARE.
 */

enum {
    NOC_FILE_DIALOG_OPEN    = 1 << 0,   // Create an open file dialog.
    NOC_FILE_DIALOG_SAVE    = 1 << 1,   // Create a save file dialog.
    NOC_FILE_DIALOG_DIR     = 1 << 2,   // Open a directory.
    NOC_FILE_DIALOG_OVERWRITE_CONFIRMATION = 1 << 3,
};

/* flags            : union of the NOC_FILE_DIALOG_XXX masks.
 * filters          : a list of strings separated by '\0' of the form:
 *                      "name1 reg1 name2 reg2 ..."
 *                    The last value is followed by two '\0'.  For example,
 *                    to filter png and jpeg files, you can use:
 *                      "png\0*.png\0jpeg\0*.jpeg\0"
 *                    You can also separate patterns with ';':
 *                      "jpeg\0*.jpg;*.jpeg\0"
 *                    Set to NULL for no filter.
 * default_path     : the default file to use or NULL.
 * default_name     : the default file name to use or NULL.
 *
 * The function return a C string.  There is no need to free it, as it is
 * managed by the library.  The string is valid until the next call to
 * no_dialog_open.  If the user canceled, the return value is NULL.
 */
const char *noc_file_dialog_open(int flags,
                                 const char *filters,
                                 const char *default_path,
                                 const char *default_name);

#include <stdlib.h>
#include <string.h>

static char *g_noc_file_dialog_ret = NULL;

#include <gtk/gtk.h>

#ifdef GDK_WINDOWING_X11
#include <gdk/gdkx.h>
#endif

const char *noc_file_dialog_open(int flags,
                                 const char *filters,
                                 const char *default_path,
                                 const char *default_name)
{
    GtkWidget *dialog;
    GtkFileFilter *filter;
    GtkFileChooser *chooser;
    GtkFileChooserAction action;
    gint res;
    char buf[128], *patterns;

    action = flags & NOC_FILE_DIALOG_SAVE ? GTK_FILE_CHOOSER_ACTION_SAVE :
                                            GTK_FILE_CHOOSER_ACTION_OPEN;
    if (flags & NOC_FILE_DIALOG_DIR)
        action = GTK_FILE_CHOOSER_ACTION_SELECT_FOLDER;

    gtk_init_check(NULL, NULL);
    dialog = gtk_file_chooser_dialog_new(
            flags & NOC_FILE_DIALOG_SAVE ? "Save File" : "Open File",
            NULL,
            action,
            "_Cancel", GTK_RESPONSE_CANCEL,
            flags & NOC_FILE_DIALOG_SAVE ? "_Save" : "_Open", GTK_RESPONSE_ACCEPT,
            NULL );
    chooser = GTK_FILE_CHOOSER(dialog);
    if (flags & NOC_FILE_DIALOG_OVERWRITE_CONFIRMATION)
        gtk_file_chooser_set_do_overwrite_confirmation(chooser, TRUE);

    if (default_path)
        gtk_file_chooser_set_filename(chooser, default_path);
    if (default_name)
        gtk_file_chooser_set_current_name(chooser, default_name);

    while (filters && *filters) {
        filter = gtk_file_filter_new();
        gtk_file_filter_set_name(filter, filters);
        filters += strlen(filters) + 1;

        // Split the filter pattern with ';'.
        strcpy(buf, filters);
        buf[strlen(buf)] = '\0';
        for (patterns = buf; *patterns; patterns++)
            if (*patterns == ';') *patterns = '\0';
        patterns = buf;
        while (*patterns) {
            gtk_file_filter_add_pattern(filter, patterns);
            patterns += strlen(patterns) + 1;
        }

        gtk_file_chooser_add_filter(chooser, filter);
        filters += strlen(filters) + 1;
    }

    gtk_widget_show_all(dialog);
#ifdef GDK_WINDOWING_X11
    if (GDK_IS_X11_DISPLAY(gdk_display_get_default())) {
        GdkWindow *window = gtk_widget_get_window(dialog);
        gdk_window_set_events(window,
            gdk_window_get_events(window) | GDK_PROPERTY_CHANGE_MASK);
        gtk_window_present_with_time(GTK_WINDOW(dialog),
            gdk_x11_get_server_time(window));
    }
#endif
    res = gtk_dialog_run(GTK_DIALOG(dialog));

    if (g_noc_file_dialog_ret != NULL) {
        free(g_noc_file_dialog_ret);
        g_noc_file_dialog_ret = NULL;
    }

    if (res == GTK_RESPONSE_ACCEPT)
        g_noc_file_dialog_ret = gtk_file_chooser_get_filename(chooser);
    gtk_widget_destroy(dialog);
    while (gtk_events_pending()) gtk_main_iteration();
    return g_noc_file_dialog_ret;
}

enum {
    SFB_MESSAGE_BOX_OK          = 1 << 0,
    SFB_MESSAGE_BOX_YESNO       = 1 << 1,
    SFB_MESSAGE_BOX_YESNOCANCEL = 1 << 2,
};

enum {
    SFB_MESSAGE_BOX_RESULT_NONE   = 1 << 0,
    SFB_MESSAGE_BOX_RESULT_OK     = 1 << 1,
    SFB_MESSAGE_BOX_RESULT_CANCEL = 1 << 2,
    SFB_MESSAGE_BOX_RESULT_YES    = 1 << 3,
    SFB_MESSAGE_BOX_RESULT_NO     = 1 << 4,
};

int sfb_message_box_open(
    int flags,
    const char *title,
    const char *caption,
    const char *window_title
) {
    GtkWidget *dialog;
    GtkMessageDialog *message_dialog;
    GtkMessageType type;
    GtkButtonsType buttons;
    GtkResponseType response;

    buttons = flags & SFB_MESSAGE_BOX_OK ? GTK_BUTTONS_OK :
              flags & SFB_MESSAGE_BOX_YESNO ? GTK_BUTTONS_YES_NO :
              GTK_BUTTONS_NONE;

    type = flags & SFB_MESSAGE_BOX_OK ? GTK_MESSAGE_INFO : GTK_MESSAGE_QUESTION;

    gtk_init_check(NULL, NULL);
    dialog = gtk_message_dialog_new(
            NULL,
            0, // TODO: GTK_DIALOG_MODAL?
            type,
            buttons,
            title);

    message_dialog = GTK_MESSAGE_DIALOG(dialog);

    // GTK has no built-in Yes/No/Cancel button set.
    if (flags & SFB_MESSAGE_BOX_YESNOCANCEL) {
        gtk_dialog_add_buttons(
            GTK_DIALOG(message_dialog),
            "_No", GTK_RESPONSE_NO,
            "_Cancel", GTK_RESPONSE_CANCEL,
            "_Yes", GTK_RESPONSE_YES,
            NULL);
    }

    if (caption)
        gtk_message_dialog_format_secondary_text(message_dialog, caption);

    gtk_widget_show_all(dialog);
#ifdef GDK_WINDOWING_X11
    if (GDK_IS_X11_DISPLAY(gdk_display_get_default())) {
        GdkWindow *window = gtk_widget_get_window(dialog);
        gdk_window_set_events(window,
            gdk_window_get_events(window) | GDK_PROPERTY_CHANGE_MASK);
        gtk_window_present_with_time(GTK_WINDOW(dialog),
            gdk_x11_get_server_time(window));
    }
#endif
    response = gtk_dialog_run(GTK_DIALOG(dialog));

    gtk_widget_destroy(dialog);
    while (gtk_events_pending()) gtk_main_iteration();

    // Translate GTK_RESPONSE to SFB_MESSAGE_BOX_RESULT
    switch (response) {
    case GTK_RESPONSE_OK:
        return SFB_MESSAGE_BOX_RESULT_OK;
    case GTK_RESPONSE_CANCEL:
        return SFB_MESSAGE_BOX_RESULT_CANCEL;
    case GTK_RESPONSE_YES:
        return SFB_MESSAGE_BOX_RESULT_YES;
    case GTK_RESPONSE_NO:
        return SFB_MESSAGE_BOX_RESULT_NO;
    case GTK_RESPONSE_CLOSE:
    case GTK_RESPONSE_APPLY:
    case GTK_RESPONSE_HELP:
    case GTK_RESPONSE_REJECT:
    case GTK_RESPONSE_ACCEPT:
    case GTK_RESPONSE_DELETE_EVENT:
    case GTK_RESPONSE_NONE:
    default:
        return SFB_MESSAGE_BOX_RESULT_NONE;
    }
}
