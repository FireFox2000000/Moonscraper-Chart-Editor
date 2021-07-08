import Cocoa
import AppKit

// MARK: set_window_title()

func set_window_title(newTitle: String) {
    NSApplication.shared.windows.first?.title = newTitle
}

@_cdecl("nativewindow_set_window_title")
public func _set_window_title(newTitle: UnsafeMutablePointer<CChar>) {
    return set_window_title(newTitle: unmarshal(newTitle))
}

// MARK: get_has_window()

@_cdecl("nativewindow_get_has_window")
public func get_has_window() -> Bool {
    return NSApplication.shared.windows.count > 0
}

// MARK: show_message_box()

enum MessageBoxType: Int {
    case ok = 0
    case yesNo
    case yesCancelNo
}

enum MessageBoxResponse: Int {
    case ok = 0
    case yes
    case no
    case cancel
}

func show_message_box(
    title: String,
    subtitle: String,
    type: MessageBoxType
) -> MessageBoxResponse {
    let alert = NSAlert()
    alert.messageText = title
    alert.informativeText = subtitle

    switch type {
    case .ok:
        break // default is a single OK button
    case .yesNo:
        alert.addButton(withTitle: "Yes")
        alert.addButton(withTitle: "No")
    case .yesCancelNo:
        alert.addButton(withTitle: "Yes")
        alert.addButton(withTitle: "Cancel")
        alert.addButton(withTitle: "No")
    }

    let response = alert.runModal()

    switch (type, response) {
    case (_, .OK), (.ok, .alertFirstButtonReturn):
        return .ok
    case (.yesNo, .alertFirstButtonReturn), (.yesCancelNo, .alertFirstButtonReturn):
        return .yes
    case (.yesNo, .alertSecondButtonReturn), (.yesCancelNo, .alertThirdButtonReturn):
        return .no
    case (.yesCancelNo, .alertSecondButtonReturn), (_, .cancel):
        return .cancel
    default:
        return .cancel
    }
}

@_cdecl("nativewindow_show_message_box")
public func _show_message_box(
    title: UnsafeMutablePointer<CChar>,
    subtitle: UnsafeMutablePointer<CChar>,
    type: Int
) -> Int {
    return show_message_box(
        title: unmarshal(title),
        subtitle: unmarshal(subtitle),
        type: MessageBoxType(rawValue: type) ?? .ok
    ).rawValue
}

// MARK: show_save_file_panel()

/// Presents a "save file" dialog and blocks until the user selects a file or cancels.
///
/// - Parameters:
///   - defaultDirectory: The initial directory for the save dialog.
///   - defaultFilename: The initial filename to populate the save dialog with.
///   - fileExtensions: The suggested file extensions to save under.
///                     If the user does not enter a file extension,
///                     the first extension in this array will be used.
/// - Returns: The selected file or empty string if the user cancelled.
func show_save_file_panel(
    defaultDirectory: String?,
    defaultFilename: String?,
    fileExtensions: [String]
) -> String {
    let panel = NSSavePanel()
    if !fileExtensions.isEmpty {
        panel.allowedFileTypes = fileExtensions
    }
    panel.allowsOtherFileTypes = true
    panel.canCreateDirectories = true
    if let directory = defaultDirectory, !directory.isEmpty {
        panel.directoryURL = URL(fileURLWithPath: directory)
    }
    if let defaultFilename = defaultFilename {
        panel.nameFieldStringValue = defaultFilename
    }

    let response = panel.runModal()

    switch response {
    case .OK:
        return panel.url?.path ?? ""
    default:
        return ""
    }
}

@_cdecl("nativewindow_show_save_file_panel")
public func _show_save_file_panel(
    defaultDirectory: UnsafeMutablePointer<CChar>,
    defaultFilename: UnsafeMutablePointer<CChar>,
    fileExtensions: UnsafeMutablePointer<CChar>
) -> UnsafePointer<CChar> {
    return marshal(show_save_file_panel(
        defaultDirectory: unmarshal(defaultDirectory),
        defaultFilename: unmarshal(defaultFilename),
        fileExtensions: parse_file_extensions(unmarshal(fileExtensions))
    ))
}

// MARK: show_open_file_panel()

/// Presents an "open file" dialog and blocks until the user selects a file or cancels.
///
/// - Parameters:
///   - defaultDirectory: The initial directory for the save dialog.
///   - defaultFilename: The initial filename to populate the save dialog with.
///   - fileExtensions: The file extensions to allow opening.
/// - Returns: The selected file or empty string if the user cancelled.
func show_open_file_panel(
    defaultDirectory: String?,
    defaultFilename: String?,
    fileExtensions: [String]
) -> String {
    let panel = NSOpenPanel()
    panel.canChooseFiles = true
    panel.canChooseDirectories = false
    panel.allowsMultipleSelection = false
    panel.canCreateDirectories = true
    if let directory = defaultDirectory, !directory.isEmpty {
        panel.directoryURL = URL(fileURLWithPath: directory)
    }
    if let defaultFilename = defaultFilename {
        panel.nameFieldStringValue = defaultFilename
    }
    if !fileExtensions.isEmpty {
        panel.allowedFileTypes = fileExtensions
    }
    panel.allowsOtherFileTypes = true

    let response = panel.runModal()

    if case .OK = response, let url = panel.urls.first {
        return url.path
    } else {
        return ""
    }
}

@_cdecl("nativewindow_show_open_file_panel")
public func _show_open_file_panel(
    defaultDirectory: UnsafeMutablePointer<CChar>,
    defaultFilename: UnsafeMutablePointer<CChar>,
    fileExtensions: UnsafeMutablePointer<CChar>
) -> UnsafePointer<CChar> {
    return marshal(show_open_file_panel(
        defaultDirectory: unmarshal(defaultDirectory),
        defaultFilename: unmarshal(defaultFilename),
        fileExtensions: parse_file_extensions(unmarshal(fileExtensions))
    ))
}

// MARK: show_open_directory_panel()

/// Presents an "open directory" dialog and blocks until the user selects a directory or cancels.
///
/// - Parameters:
///   - defaultDirectory: The initial directory for the dialog.
/// - Returns: The selected file or empty string if the user cancelled.
func show_open_directory_panel(
    defaultDirectory: String?
) -> String {
    let panel = NSOpenPanel()
    panel.canChooseFiles = false
    panel.canChooseDirectories = true
    panel.allowsMultipleSelection = false
    panel.canCreateDirectories = true
    if let directory = defaultDirectory, !directory.isEmpty {
        panel.directoryURL = URL(fileURLWithPath: directory)
    }

    let response = panel.runModal()

    if case .OK = response, let url = panel.urls.first {
        return url.path
    } else {
        return ""
    }
}

@_cdecl("nativewindow_show_open_directory_panel")
public func _show_open_directory_panel(
    defaultDirectory: UnsafeMutablePointer<CChar>
) -> UnsafePointer<CChar> {
    return marshal(show_open_directory_panel(
        defaultDirectory: unmarshal(defaultDirectory)
    ))
}

// MARK: File Extension List Parser

func parse_file_extensions(_ extensionString: String) -> [String] {
    return extensionString.split(separator: ",").map { String($0) }
}

// MARK: Marshalling

func marshal(_ string: String) -> UnsafePointer<CChar> {
    let copy: UnsafeMutablePointer<CChar> = strdup(string)
    return UnsafePointer(copy)
}

func unmarshal(_ cString: UnsafeMutablePointer<CChar>) -> String {
    let value = String(cString: cString)
    free(cString)
    return value
}
