# DigiKam Face Recognition Database Reader

A C# console application that extracts face tagging data from DigiKam databases for use in facial recognition systems, machine learning pipelines, and photo organization workflows.

## Overview

DigiKam is a professional photo management application that includes advanced face recognition capabilities. This tool extracts and consolidates face tagging data from DigiKam's SQLite databases, making it easy to:

- **Build ML Training Datasets**: Export tagged photos for training custom facial recognition models
- **Organize Photo Collections**: Quickly locate all photos of specific individuals
- **Analyze Face Tagging**: Understand your photo collection's face recognition coverage
- **Integrate with Pipelines**: Generate file lists for batch processing in ML workflows

## Features

- **Database Statistics**: View comprehensive metrics about identities, tags, and images
- **Person Discovery**: List all recognized people in your photo collection
- **Smart Search**: Find people by name with fuzzy/partial matching
- **Image Retrieval**: Locate all photos containing specific individuals with full metadata
- **Export Functionality**: Generate text files of image paths for downstream processing
- **Safe Read-Only Access**: Opens databases in read-only mode to prevent accidental modifications
- **Multi-Database Support**: Consolidates data from multiple DigiKam databases:
  - `recognition.db` - Face recognition data and person identities
  - `digikam4.db` - Main image database with tags and metadata
  - `thumbnails-digikam.db` - Thumbnail storage

## Requirements

- .NET 6.0 SDK or later
- DigiKam photo management software (to generate the databases)
- DigiKam database files in accessible location
- Windows, Linux, or macOS

## Installation

1. Clone or download this repository
2. Navigate to the project directory:
   ```bash
   cd facerecog_Db_reader
   ```
3. Build the project:
   ```bash
   dotnet build
   ```

## Configuration

The application looks for DigiKam databases in the following default location:
- **Windows**: `%USERPROFILE%\Pictures\DH`
- **Linux/macOS**: `$HOME/Pictures/DH`

If your DigiKam databases are in a different location, update the path in `Program.cs:8`.

## Database Structure

DigiKam stores data across multiple SQLite databases:

### recognition.db
- **Identities**: Person IDs and types
- **IdentityAttributes**: Person names and attributes (join on `id` where `attribute='name'`)
- **FaceMatrices**: Face embeddings/encodings linked to identities

### digikam4.db
- **Images**: Image file information and metadata
- **Albums**: Album/folder hierarchical structure
- **AlbumRoots**: Base paths for album collections
- **Tags**: All tags including person names created by face recognition
- **ImageTags**: Many-to-many relationship linking images to tags
- **ImageInformation**: Extended metadata (creation dates, ratings, etc.)

## Usage

1. Build the project:
   ```bash
   cd facerecog_Db_reader
   dotnet build
   ```

2. Run the application:
   ```bash
   dotnet run
   ```

3. Menu options:
   - **1. List all people**: Display all unique person names from the database
   - **2. Search for a person**: Search for people by name pattern
   - **3. Find images by person name**: Get all images containing a specific person
   - **4. Exit**: Close the application

## Example Workflow

1. Start the application - it will show database statistics
2. Choose option 2 to search for a person (e.g., enter "John")
3. Once you find the exact name, use option 3 to find all images
4. Export the image paths to a text file for further processing

## Project Structure

```
facerecog_Db_reader/
├── Models/
│   ├── Identity.cs          # Person identity model
│   ├── IdentityAttribute.cs # Person attributes
│   ├── Image.cs             # Image metadata
│   └── Tag.cs               # Tag information
├── FaceRecognitionDbReader.cs  # Main database reader service
├── Program.cs               # Console interface
└── TODO.md                  # Project roadmap
```

## Implementation Phases

### Phase 1: Read the Database ✓
- Implemented SQLite database reading
- Created data models for all entities
- Added database schema exploration

### Phase 2: Analyze the Data ✓
- Statistics collection
- Person search functionality
- Data aggregation across multiple databases

### End Goal: Search Images by Person ✓
- Search for people by name
- Retrieve all images for a person
- Export image paths to files

## Notes

- The application opens databases in read-only mode for safety
- Image paths are constructed from AlbumRoots, Albums, and image names
- Names can come from either the recognition.db Identities or digikam4.db Tags
