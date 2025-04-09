-- Create episodes table
CREATE TABLE IF NOT EXISTS episodes (
    id VARCHAR(10) PRIMARY KEY,
    season_number INTEGER NOT NULL,
    episode_number INTEGER NOT NULL,
    title VARCHAR(255) NOT NULL,
    description TEXT NOT NULL,
    UNIQUE(season_number, episode_number)
);
